using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Queries.GetAll;
using VOID.Domain.Models.Chats;
using VOID.Domain.Models.Groups;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using VOID.Shared.Contracts.DTOs.Paginations;
using VOID.Shared.Contracts.Enums.Chats;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Messages.Queries.GetAll;

public sealed class GetMessagesByParentQueryHandlerTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMapper _mapper;

    private readonly GetMessagesByParentQueryHandler _sut;

    public GetMessagesByParentQueryHandlerTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _chatRepository = Substitute.For<IChatRepository>();
        _groupRepository = Substitute.For<IGroupRepository>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetMessagesByParentQueryHandler(
            _messageRepository,
            _chatRepository,
            _groupRepository,
            _encryptionService,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenChatTypeIsUnsupported()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        var query = new GetMessagesByParentQuery(
            parentId,
            userId,
            (ChatType)999,
            pagination);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenPrivateChatDoesNotExist()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        _messageRepository
            .GetTotalCountByChatAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(0);

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns((Chat?)null);

        var query = new GetMessagesByParentQuery(
            chatId,
            userId,
            ChatType.Private,
            pagination);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _chatRepository
            .Received(1)
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfPrivateChat()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = otherUserId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _messageRepository
            .GetTotalCountByChatAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(0);

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        var query = new GetMessagesByParentQuery(
            chatId,
            userId,
            ChatType.Private,
            pagination);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        _messageRepository
            .GetTotalCountByGroupAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns((GroupChat?)null);

        var query = new GetMessagesByParentQuery(
            groupId,
            userId,
            ChatType.Group,
            pagination);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = otherUserId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = otherUserId,
            GroupRole = VOID.Domain.Enums.Roles.Group.GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        _messageRepository
            .GetTotalCountByGroupAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(group);

        var query = new GetMessagesByParentQuery(
            groupId,
            userId,
            ChatType.Group,
            pagination);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPrivateMessages_WhenUserIsMember()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var totalCount = 2;

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = userId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = recipientId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        var messages = new List<Message>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Text = "encrypted-message-1",
                SenderId = userId,
                ChatId = chatId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Text = "encrypted-message-2",
                SenderId = recipientId,
                ChatId = chatId,
                CreatedAt = DateTime.UtcNow
            }
        };

        var messageDtos = new List<MessageDto>
        {
            new()
            {
                Id = messages[0].Id,
                Text = "decrypted-message-1",
                SenderId = userId
            },
            new()
            {
                Id = messages[1].Id,
                Text = "decrypted-message-2",
                SenderId = recipientId
            }
        };

        _messageRepository
            .GetTotalCountByChatAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(totalCount);

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetMessagesByParentAsync(
                chatId,
                VOID.Domain.Enums.Types.Chat.ChatType.Private,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(messages);

        _encryptionService
            .Decrypt("encrypted-message-1")
            .Returns("decrypted-message-1");

        _encryptionService
            .Decrypt("encrypted-message-2")
            .Returns("decrypted-message-2");

        _mapper
            .Map<List<MessageDto>>(
                messages,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDtos);

        var query = new GetMessagesByParentQuery(
            chatId,
            userId,
            ChatType.Private,
            pagination);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        _encryptionService
            .Received(2)
            .Decrypt(Arg.Any<string>());

        _mapper
            .Received(1)
            .Map<List<MessageDto>>(
                messages,
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnGroupMessages_WhenUserIsMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var totalCount = 1;

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = userId,
            GroupRole = VOID.Domain.Enums.Roles.Group.GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        var messages = new List<Message>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Text = "encrypted-group-message",
                SenderId = userId,
                GroupChatId = groupId,
                CreatedAt = DateTime.UtcNow
            }
        };

        var messageDtos = new List<MessageDto>
        {
            new()
            {
                Id = messages[0].Id,
                Text = "decrypted-group-message",
                SenderId = userId
            }
        };

        _messageRepository
            .GetTotalCountByGroupAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(totalCount);

        _groupRepository
            .GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(group);

        _messageRepository
            .GetMessagesByParentAsync(
                groupId,
                VOID.Domain.Enums.Types.Chat.ChatType.Group,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(messages);

        _encryptionService
            .Decrypt("encrypted-group-message")
            .Returns("decrypted-group-message");

        _mapper
            .Map<List<MessageDto>>(
                messages,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDtos);

        var query = new GetMessagesByParentQuery(
            groupId,
            userId,
            ChatType.Group,
            pagination);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(totalCount);

        _encryptionService
            .Received(1)
            .Decrypt("encrypted-group-message");
    }

    [Fact]
    public async Task Handle_ShouldDecryptMessages_WhenMessagesHaveText()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = userId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = recipientId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        var messages = new List<Message>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Text = "encrypted-text-1",
                SenderId = userId,
                ChatId = chatId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Text = null, // Нет текста
                SenderId = userId,
                ChatId = chatId,
                CreatedAt = DateTime.UtcNow
            }
        };

        _messageRepository
            .GetTotalCountByChatAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(2);

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetMessagesByParentAsync(
                chatId,
                VOID.Domain.Enums.Types.Chat.ChatType.Private,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(messages);

        _encryptionService
            .Decrypt("encrypted-text-1")
            .Returns("decrypted-text-1");

        _mapper
            .Map<List<MessageDto>>(
                messages,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(new List<MessageDto>());

        var query = new GetMessagesByParentQuery(
            chatId,
            userId,
            ChatType.Private,
            pagination);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _encryptionService
            .Received(1)
            .Decrypt("encrypted-text-1");

        _encryptionService
            .DidNotReceive()
            .Decrypt(Arg.Is<string>(s => s == null));
    }

    [Fact]
    public async Task Handle_ShouldUsePaginationParameters_WhenGettingMessages()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var pagination = new PaginationRequest(2, 5);

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = userId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = recipientId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _messageRepository
            .GetTotalCountByChatAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(10);

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetMessagesByParentAsync(
                chatId,
                VOID.Domain.Enums.Types.Chat.ChatType.Private,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(new List<Message>());

        _mapper
            .Map<List<MessageDto>>(
                Arg.Any<List<Message>>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(new List<MessageDto>());

        var query = new GetMessagesByParentQuery(
            chatId,
            userId,
            ChatType.Private,
            pagination);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);

        await _messageRepository
            .Received(1)
            .GetMessagesByParentAsync(
                chatId,
                VOID.Domain.Enums.Types.Chat.ChatType.Private,
                Arg.Is<PaginationRequest>(p =>
                    p.PageNumber == 2 &&
                    p.PageSize == 5),
                Arg.Any<CancellationToken>());
    }
}