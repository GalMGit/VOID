using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.UseCases.Chats.Queries.GetChatsByUser;
using VOID.Domain.Models.Chats;
using VOID.Shared.Contracts.DTOs.Chats;
using VOID.Shared.Contracts.DTOs.Paginations;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Chats.Queries.GetChatsByUser;

public sealed class GetChatsByUserQueryHandlerTests
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMapper _mapper;

    private readonly GetChatsByUserQueryHandler _sut;

    public GetChatsByUserQueryHandlerTests()
    {
        _chatRepository = Substitute.For<IChatRepository>();
        _messageRepository = Substitute.For<IMessageRepository>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetChatsByUserQueryHandler(
            _chatRepository,
            _messageRepository,
            _encryptionService,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult_WhenChatsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var chatId1 = Guid.NewGuid();
        var chatId2 = Guid.NewGuid();
        var totalCount = 2;

        var chats = new List<Chat>
        {
            new()
            {
                Id = chatId1,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow,
                Interlocutors = new List<ChatInterlocutor>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ChatId = chatId1,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            },
            new()
            {
                Id = chatId2,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow,
                Interlocutors = new List<ChatInterlocutor>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ChatId = chatId2,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            }
        };

        var chatDtos = new List<ChatDto>
        {
            new() { Id = chatId1, LastMessage = "encrypted1" },
            new() { Id = chatId2, LastMessage = "encrypted2" }
        };

        var unreadCounts = new Dictionary<Guid, int>
        {
            { chatId1, 3 },
            { chatId2, 0 }
        };

        _chatRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(totalCount);

        _chatRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(chats);

        _messageRepository
            .GetUnreadCountsAsync(
                userId,
                Arg.Is<List<Guid>>(ids => 
                    ids.Count == 2 && 
                    ids.Contains(chatId1) && 
                    ids.Contains(chatId2)),
                Arg.Any<CancellationToken>())
            .Returns(unreadCounts);

        _mapper
            .Map<List<ChatDto>>(
                chats,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDtos);

        _encryptionService
            .Decrypt("encrypted1")
            .Returns("decrypted1");

        _encryptionService
            .Decrypt("encrypted2")
            .Returns("decrypted2");

        var query = new GetChatsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        await _chatRepository
            .Received(1)
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>());

        await _messageRepository
            .Received(1)
            .GetUnreadCountsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<List<ChatDto>>(
                chats,
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldDecryptLastMessages_WhenLastMessageExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var chatId = Guid.NewGuid();
        var encryptedMessage = "encrypted-message";
        var decryptedMessage = "decrypted-message";

        var chats = new List<Chat>
        {
            new()
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
                    }
                }
            }
        };

        var chatDtos = new List<ChatDto>
        {
            new() { Id = chatId, LastMessage = encryptedMessage }
        };

        var unreadCounts = new Dictionary<Guid, int>
        {
            { chatId, 0 }
        };

        _chatRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(1);

        _chatRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(chats);

        _messageRepository
            .GetUnreadCountsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(unreadCounts);

        _mapper
            .Map<List<ChatDto>>(
                chats,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDtos);

        _encryptionService
            .Decrypt(encryptedMessage)
            .Returns(decryptedMessage);

        var query = new GetChatsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Items[0].LastMessage.Should().Be(decryptedMessage);

        _encryptionService
            .Received(1)
            .Decrypt(encryptedMessage);
    }

    [Fact]
    public async Task Handle_ShouldNotDecrypt_WhenLastMessageIsNullOrEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var chatId1 = Guid.NewGuid();
        var chatId2 = Guid.NewGuid();

        var chats = new List<Chat>
        {
            new()
            {
                Id = chatId1,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow,
                Interlocutors = new List<ChatInterlocutor>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ChatId = chatId1,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            },
            new()
            {
                Id = chatId2,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow,
                Interlocutors = new List<ChatInterlocutor>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ChatId = chatId2,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            }
        };

        var chatDtos = new List<ChatDto>
        {
            new() { Id = chatId1, LastMessage = null },
            new() { Id = chatId2, LastMessage = "" }
        };

        var unreadCounts = new Dictionary<Guid, int>
        {
            { chatId1, 0 },
            { chatId2, 0 }
        };

        _chatRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(2);

        _chatRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(chats);

        _messageRepository
            .GetUnreadCountsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(unreadCounts);

        _mapper
            .Map<List<ChatDto>>(
                chats,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDtos);

        var query = new GetChatsByUserQuery(userId, pagination);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        _encryptionService
            .DidNotReceive()
            .Decrypt(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldSetUnreadCounts_WhenReturningChats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var chatId1 = Guid.NewGuid();
        var chatId2 = Guid.NewGuid();

        var chats = new List<Chat>
        {
            new()
            {
                Id = chatId1,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow,
                Interlocutors = new List<ChatInterlocutor>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ChatId = chatId1,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            },
            new()
            {
                Id = chatId2,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow,
                Interlocutors = new List<ChatInterlocutor>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ChatId = chatId2,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            }
        };

        var chatDtos = new List<ChatDto>
        {
            new() { Id = chatId1 },
            new() { Id = chatId2 }
        };

        var unreadCounts = new Dictionary<Guid, int>
        {
            { chatId1, 5 },
            { chatId2, 2 }
        };

        _chatRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(2);

        _chatRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(chats);

        _messageRepository
            .GetUnreadCountsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(unreadCounts);

        _mapper
            .Map<List<ChatDto>>(
                chats,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDtos);

        var query = new GetChatsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Items[0].UnreadCount.Should().Be(5);
        result.Items[1].UnreadCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoChatsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        _chatRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _chatRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(new List<Chat>());

        _messageRepository
            .GetUnreadCountsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());

        _mapper
            .Map<List<ChatDto>>(
                Arg.Any<List<Chat>>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(new List<ChatDto>());

        var query = new GetChatsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectChatIds_WhenGettingUnreadCounts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var chatId1 = Guid.NewGuid();
        var chatId2 = Guid.NewGuid();
        var chatId3 = Guid.NewGuid();

        var chats = new List<Chat>
        {
            new() { Id = chatId1, CreatorId = userId, CreatedAt = DateTime.UtcNow },
            new() { Id = chatId2, CreatorId = userId, CreatedAt = DateTime.UtcNow },
            new() { Id = chatId3, CreatorId = userId, CreatedAt = DateTime.UtcNow }
        };

        var chatDtos = new List<ChatDto>
        {
            new() { Id = chatId1 },
            new() { Id = chatId2 },
            new() { Id = chatId3 }
        };

        _chatRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(3);

        _chatRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(chats);

        _messageRepository
            .GetUnreadCountsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());

        _mapper
            .Map<List<ChatDto>>(
                chats,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDtos);

        var query = new GetChatsByUserQuery(userId, pagination);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .GetUnreadCountsAsync(
                userId,
                Arg.Is<List<Guid>>(ids =>
                    ids.Count == 3 &&
                    ids.Contains(chatId1) &&
                    ids.Contains(chatId2) &&
                    ids.Contains(chatId3)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUsePaginationParameters_WhenGettingChats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(3, 20);

        _chatRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _chatRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(new List<Chat>());

        _messageRepository
            .GetUnreadCountsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());

        _mapper
            .Map<List<ChatDto>>(
                Arg.Any<List<Chat>>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(new List<ChatDto>());

        var query = new GetChatsByUserQuery(userId, pagination);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .GetAllByUserAsync(
                userId,
                Arg.Is<PaginationRequest>(p =>
                    p.PageNumber == 3 &&
                    p.PageSize == 20),
                Arg.Any<CancellationToken>());
    }
}