using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Extensions;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Events.Deleted;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using VOID.Shared.Contracts.Enums.Messages;
using Wolverine;
using DomainChatType = VOID.Domain.Enums.Types.Chat.ChatType;


namespace VOID.Application.UseCases.Messages.Commands.Delete;

public sealed class DeleteMessageCommandHandler(
    IMessageRepository messageRepository,
    IFileStorageService fileStorageService,
    IMessageBus bus,
    IEncryptionService encryptionService,
    IMapper mapper,
    IChatRepository chatRepository)
{
    public async Task Handle( 
        DeleteMessageCommand request, 
        CancellationToken ct)
    {
        var message = await GetMessageAsync(
            request, ct);

        await messageRepository.DeleteAsync(
            request.MessageId, ct);
        
        await DeleteMediaAsync(
            message.MediaUrl,
            message.MessageType.ToShared(),
            message.ThumbnailUrl);

        await PublishDeleteEventAsync(
            message, 
            request, ct);
    }
    
    private async Task PublishDeleteEventAsync(
        Message message,
        DeleteMessageCommand request,
        CancellationToken ct)
    {
        switch (message.ChatType)
        {
            case DomainChatType.Private when message.ChatId.HasValue:
                await PublishPrivateDeleteEventAsync(
                    message,
                    request,
                    ct);
                break;

            case DomainChatType.Group when message.GroupChatId.HasValue:
                await bus.PublishAsync(
                    new GroupMessageDeletedEvent(
                        message.GroupChatId.Value,
                        request.MessageId,
                        request.UserId));
                break;
        }
    }

    private async Task<Message> GetMessageAsync(
        DeleteMessageCommand request, 
        CancellationToken ct)
    {
        var message = await messageRepository.GetByIdAsync(
            request.MessageId, ct);

        if (message is null)
            throw new NotFoundException("Сообещение не найдено");

        if (message.SenderId != request.UserId)
            throw new ForbiddenException();

        return message;
    }

    private async Task DeleteMediaAsync(
        string? mediaUrl, 
        MessageType messageType, 
        string? thumbnailUrl)
    {
        if (messageType is MessageType.Image or MessageType.Video 
            && !string.IsNullOrWhiteSpace(mediaUrl) 
            && !string.IsNullOrWhiteSpace(thumbnailUrl))
        {
            await fileStorageService.DeleteMediaAsync(
                mediaUrl,
                thumbnailUrl);
        }
    }
    
    private async Task PublishPrivateDeleteEventAsync(
        Message message,
        DeleteMessageCommand request,
        CancellationToken ct)
    {
        var chatId = message.ChatId!.Value;

        var newLastMessage = await messageRepository.GetLastMessageAsync(
            chatId,
            ct);

        await chatRepository.UpdateLastMessageAsync(
            chatId,
            newLastMessage?.Text,
            newLastMessage?.CreatedAt,
            ct);

        var recipientId = await chatRepository.GetRecipientIdAsync(
            request.UserId,
            chatId,
            ct);

        var dto = MapLastMessage(newLastMessage, request.UserId);

        await bus.PublishAsync(
            new PrivateMessageDeletedEvent(
                recipientId,
                chatId,
                request.MessageId,
                request.UserId,
                dto));
    }
    
    private MessageDto? MapLastMessage(
        Message? message,
        Guid currentUserId)
    {
        if (message is null)
            return null;

        var dto = mapper.Map<MessageDto>(
            message,
            opt => opt.Items["CurrentUserId"] = currentUserId);

        if (!string.IsNullOrWhiteSpace(dto.Text))
            dto.Text = encryptionService.Decrypt(dto.Text);

        return dto;
    }
}