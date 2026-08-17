using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Events.Updated;
using VOID.Domain.Enums.Types.Chat;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;

namespace VOID.Application.UseCases.Messages.Commands.Update;

public sealed class UpdateMessageCommandHandler(
    IMessageRepository messageRepository,
    IChatRepository chatRepository,
    IMapper mapper,
    IEncryptionService encryptionService,
    IMessageBus bus)
{
    public async Task Handle(
        UpdateMessageCommand request, 
        CancellationToken ct)
    {
        var message = await messageRepository.GetByIdAsync(
                          request.MessageId, ct)
                      ?? throw new NotFoundException("Сообщение не найдено");

        if (message.SenderId != request.UserId)
            throw new ForbiddenException();

        if (!string.IsNullOrWhiteSpace(request.Dto.Text))
        {
            message.Text = request.Dto.Text;
            message.IsEdited = true;
        }

        if (!string.IsNullOrWhiteSpace(message.Text))
            message.Text = encryptionService.Encrypt(message.Text);

        var updatedMessage = await messageRepository.UpdateAsync(
            message, ct);

        if (!string.IsNullOrWhiteSpace(updatedMessage.Text))
            updatedMessage.Text = encryptionService.Decrypt(updatedMessage.Text);
        
        var messageDto = mapper.Map<MessageDto>(
            updatedMessage,
            opt => opt.Items["CurrentUserId"] = request.UserId);

        if (message.ChatType == ChatType.Private)
        {
            var recipientId = await chatRepository.GetRecipientIdAsync(
                    request.UserId,
                    message.ChatId!.Value, ct);
            
            await bus.PublishAsync(
                new PrivateMessageUpdatedEvent(
                    messageDto,
                    message.ChatId!.Value,
                    recipientId,
                    request.UserId));
        }
        else
        {
            await bus.PublishAsync(
                new GroupMessageUpdatedEvent(
                    messageDto,
                    message.GroupChatId!.Value,
                    request.UserId));
        }
    }
}