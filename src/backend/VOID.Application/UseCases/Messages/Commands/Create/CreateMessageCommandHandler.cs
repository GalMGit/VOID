using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ImTools;
using VOID.Application.Extensions;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Events.Created;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;
using DomainChatType = VOID.Domain.Enums.Types.Chat.ChatType;
using DomainMessageType = VOID.Domain.Enums.Types.Message.MessageType;

namespace VOID.Application.UseCases.Messages.Commands.Create;

public sealed class CreateMessageCommandHandler(
    IMessageRepository messageRepository,
    IChatRepository chatRepository,
    IGroupRepository groupRepository,
    IFileStorageService storageService,
    IEncryptionService encryptionService,
    IMessageBus bus,
    IMapper mapper) 
{
    public async Task<MessageDto> Handle(
        CreateMessageCommand request, 
        CancellationToken ct)
    {
        var chatType = request.Dto.ChatType.ToDomain();
        var messageType = request.Dto.MessageType.ToDomain();

        await ValidateAccessAsync(
            request.Dto.ParentId, 
            request.UserId,
            chatType, ct);

        var (mediaUrl, thumbnailUrl, contentType) = await UploadMediaAsync(
            request, ct);

        var message = CreateMessage(
            request,
            chatType,
            messageType,
            mediaUrl,
            thumbnailUrl,
            contentType);
        
        var createdMessage = await messageRepository.CreateAsync(
            message, 
            ct);

        await UpdateLastMessageAsync(
            request,
            chatType,
            messageType, ct);
        
        var dto = MapMessage(
            createdMessage,
            request.UserId,
            request.Dto.Text);

        await PublishMessageCreatedAsync(
            request,
            chatType,
            dto,
            ct);

        return dto;
    }

    private async Task ValidateAccessAsync(
        Guid parentId, 
        Guid userId, 
        DomainChatType chatType, 
        CancellationToken ct)
    {
        switch (chatType)
        {
            case DomainChatType.Private:
                if (!await chatRepository.ExistsAsync(parentId, ct))
                    throw new NotFoundException("Чат не найден");

                if (!await chatRepository.IsMemberAsync(parentId, userId, ct))
                    throw new ForbiddenException();
                
                break;
            
            case DomainChatType.Group:
                if (!await groupRepository.ExistsAsync(parentId, ct))
                    throw new NotFoundException("Группа не найдена");

                if (!await groupRepository.IsMemberAsync(parentId, userId, ct))
                    throw new ForbiddenException();
                
                break;
            
            default:
                throw new NotFoundException("Неизвестный тип чата");
        }
    }

    private async Task<(
        string? MediaUrl, 
        string? ThumbnailUrl, 
        string? ContentType)> UploadMediaAsync(
        CreateMessageCommand request,
        CancellationToken ct)
    {
        if (request.Media is null)
            return (null, null, null);

        var upload = await storageService.UploadMessageMediaAsync(
            request.Media, 
            request.Dto.ParentId, ct);

        return (
            upload.RelativePath, 
            upload.ThumbnailRelativePath, 
            upload.ContentType
            );
    }

    private Message CreateMessage(
        CreateMessageCommand request,
        DomainChatType chatType,
        DomainMessageType messageType,
        string? mediaUrl,
        string? thumbnailUrl,
        string? contentType)
    {
        var encryptedText = string.IsNullOrWhiteSpace(request.Dto.Text)
            ? null
            : encryptionService.Encrypt(request.Dto.Text);
        
        var message = new Message
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            MediaUrl = mediaUrl,
            ThumbnailUrl = thumbnailUrl,
            MessageType = messageType,
            ChatType = chatType,
            ContentType = contentType,
            Text = encryptedText,
            SenderId = request.UserId
        };
        
        if (chatType == DomainChatType.Private)
            message.ChatId = request.Dto.ParentId;
        else
            message.GroupChatId = request.Dto.ParentId;

        return message;
    }
    
    private async Task UpdateLastMessageAsync(
        CreateMessageCommand request,
        DomainChatType chatType,
        DomainMessageType messageType,
        CancellationToken ct)
    {
        if (chatType != DomainChatType.Private)
            return;

        var preview = messageType switch
        {
            DomainMessageType.Text => request.Dto.Text,
            DomainMessageType.Image => "📷 Фото",
            DomainMessageType.Video => "🎥 Видео",
            DomainMessageType.Audio => "Аудио",
            _ => "Сообщение"
        };

        await chatRepository.UpdateLastMessageAsync(
            request.Dto.ParentId,
            encryptionService.Encrypt(preview!),
            DateTime.UtcNow,
            ct);
    }
    
    private MessageDto MapMessage(
        Message message,
        Guid currentUserId,
        string? text)
    {
        var dto = mapper.Map<MessageDto>(
            message,
            opt => opt.Items["CurrentUserId"] = currentUserId);

        dto.Text = text;

        return dto;
    }
    
    private async Task PublishMessageCreatedAsync(
        CreateMessageCommand request,
        DomainChatType chatType,
        MessageDto dto,
        CancellationToken ct)
    {
        if (chatType == DomainChatType.Private)
        {
            var recipientId = await chatRepository.GetRecipientIdAsync(
                request.UserId,
                request.Dto.ParentId,
                ct);

            await bus.PublishAsync(
                new PrivateMessageCreatedEvent(dto, recipientId));

            return;
        }

        await bus.PublishAsync(
            new GroupMessageCreatedEvent(dto));
    }
}