using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Chats.Events.Created;
using VOID.Domain.Models.Chats;
using VOID.Shared.Contracts.DTOs.Chats;
using Wolverine;

namespace VOID.Application.UseCases.Chats.Commands.Create;

public sealed class CreateChatCommandHandler(
    IChatRepository chatRepository,
    IUserRepository userRepository,
    IMessageBus bus,
    IMapper mapper)
{
    public async Task<ChatDto> Handle(
        CreateChatCommand request,
        CancellationToken ct)
    {
        var chat = await CreateChatEntityAsync(
            request.Dto,
            request.UserId,
            ct);

        var recipientId = chat.Interlocutors
            .First(x => x.UserId != request.UserId).UserId;

        var creatorChat = mapper.Map<ChatDto>(chat, opt =>
            opt.Items["CurrentUserId"] = request.UserId);

        var recipientChat = mapper.Map<ChatDto>(chat, opt =>
            opt.Items["CurrentUserId"] = recipientId);

        await bus.PublishAsync(
            new ChatCreatedEvent(
                chat.Id,
                request.UserId,
                recipientId,
                creatorChat,
                recipientChat));

        return creatorChat;
    }

    private async Task<Chat> CreateChatEntityAsync(
        CreateChatDto dto,
        Guid userId,
        CancellationToken ct = default)
    {
        var targetUser = await userRepository.GetByUsernameAsync(
                                 dto.Username, ct)
            ?? throw new NotFoundException("Указанный пользователь не найден");

        if (userId == targetUser.Id)
            throw new ConflictException("Нельзя создать чат с самим собой");

        if (await chatRepository.ExistsBetweenUsersAsync(
                userId,
                targetUser.Id, ct))
            throw new ConflictException("Чат уже существует");

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatorId = userId
        };

        chat.Interlocutors = [
            new ChatInterlocutor
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = chat.CreatedAt,
                ChatId = chat.Id
            },
            new ChatInterlocutor
            {
                Id = Guid.NewGuid(),
                UserId = targetUser.Id,
                CreatedAt = chat.CreatedAt,
                ChatId = chat.Id
            }
        ];

        return await chatRepository.CreateAsync(
            chat, ct);
    }
}