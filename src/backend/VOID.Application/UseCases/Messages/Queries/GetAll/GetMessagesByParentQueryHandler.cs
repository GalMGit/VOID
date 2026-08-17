using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Extensions;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Shared.Contracts.DTOs.Messages;
using VOID.Shared.Contracts.DTOs.Paginations;
using DomainChatType = VOID.Domain.Enums.Types.Chat.ChatType;

namespace VOID.Application.UseCases.Messages.Queries.GetAll;

public sealed class GetMessagesByParentQueryHandler(
    IMessageRepository messageRepository,
    IChatRepository chatRepository,
    IGroupRepository groupRepository,
    IEncryptionService encryptionService,
    IMapper mapper)
{
    public async Task<PaginatedResult<MessageDto>> Handle(
        GetMessagesByParentQuery request, 
        CancellationToken ct)
    {
        var chatType = request.ChatType.ToDomain();
        
        return chatType switch
        {
            DomainChatType.Private => await GetPrivateMessagesAsync(
                request.ParentId, 
                request.UserId, 
                request.Pagination, ct),
            DomainChatType.Group => await GetGroupMessagesAsync(
                request.ParentId, 
                request.UserId, 
                request.Pagination, ct),
            _ => throw new ValidationException($"Неподдерживаемый тип чата: {chatType}")
        };
    }
    
    private async Task<PaginatedResult<MessageDto>> GetPrivateMessagesAsync(
        Guid chatId,
        Guid userId,
        PaginationRequest pagination,
        CancellationToken ct)
    {
        var totalCount = await messageRepository.GetTotalCountByChatAsync(
            chatId, ct);

        var chat = await chatRepository.GetByIdAsync(chatId, ct)
                   ?? throw new NotFoundException("Чат не найден");

        if (chat.Interlocutors
            .All(x => x.UserId != userId))
            throw new ForbiddenException();

        var messages = await messageRepository.GetMessagesByParentAsync(
            chatId, 
            DomainChatType.Private, 
            pagination, ct);

        foreach (var message in messages)
        {
            if (!string.IsNullOrWhiteSpace(message.Text))
                message.Text = encryptionService.Decrypt(message.Text);
        }

        var messagesResponse = mapper.Map<List<MessageDto>>(messages,
            opts => opts.Items["CurrentUserId"] = userId);

        return new PaginatedResult<MessageDto>(
            messagesResponse,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize
        );
    }

    private async Task<PaginatedResult<MessageDto>> GetGroupMessagesAsync(
        Guid groupId, 
        Guid userId, 
        PaginationRequest pagination, 
        CancellationToken ct)
    {
        var totalCount = await messageRepository.GetTotalCountByGroupAsync(
            groupId, ct);

        var group = await groupRepository.GetByIdAsync(
                        groupId, ct)
                    ?? throw new NotFoundException("Группа не найдена");

        if (group.GroupMembers
            .All(x => x.MemberId != userId))
            throw new ForbiddenException();

        var messages = await messageRepository.GetMessagesByParentAsync(
            groupId, 
            DomainChatType.Group, 
            pagination, ct);
        
        foreach (var message in messages)
        {
            if (!string.IsNullOrWhiteSpace(message.Text))
                message.Text = encryptionService.Decrypt(message.Text);
        }
        
        var messagesResponse = mapper.Map<List<MessageDto>>(messages,
            opts => opts.Items["CurrentUserId"] = userId);

        return new PaginatedResult<MessageDto>(
            messagesResponse,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize
        );
    }
}