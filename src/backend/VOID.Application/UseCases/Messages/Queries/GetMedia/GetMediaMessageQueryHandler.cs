using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Exceptions;
using VOID.Domain.Enums.Types.Chat;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Queries.GetMedia;

public sealed class GetMediaMessageQueryHandler(
    IMessageRepository messageRepository,
    IChatRepository chatRepository,
    IFileStorageService fileStorageService,
    IGroupRepository groupRepository)
{
    public async Task<MessageMediaResult> Handle(
        GetMediaMessageQuery request,
        CancellationToken ct)
    {
        var message = await messageRepository.GetByIdAsync(
                          request.MessageId, ct) 
                      ?? throw new NotFoundException("Сообщение не найдено");

        switch (message.ChatType)
        {
            case ChatType.Private:
            {
                if (!await chatRepository.IsMemberAsync(
                        message.ChatId!.Value,
                        request.UserId, ct))
                    throw new ForbiddenException();
                break;
            }
            case ChatType.Group:
            {
                if (!await groupRepository.IsMemberAsync(
                        message.GroupChatId!.Value, 
                        request.UserId, ct))
                    throw new ForbiddenException();
                break;
            }
            default:
                throw new NotFoundException("Неизвестный тип чата");
        }

        if (string.IsNullOrWhiteSpace(message.MediaUrl))
            throw new NotFoundException("У сообщения нет медиа");

        var url = fileStorageService.GetMessageMediaUrl(
            message.MediaUrl, 
            TimeSpan.FromMinutes(10));

        return new MessageMediaResult(
            url,
            message.ContentType!);
    }
}