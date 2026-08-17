using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Chats.Queries.GetWithUser;
using VOID.Domain.Models.Chats;
using VOID.Shared.Contracts.DTOs.Chats;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Chats.Queries.GetWithUser;

public sealed class GetPrivateChatQueryHandlerTests
{
    private readonly IMapper _mapper;
    private readonly IChatRepository _chatRepository;

    private readonly GetPrivateChatQueryHandler _sut;

    public GetPrivateChatQueryHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _chatRepository = Substitute.For<IChatRepository>();

        _sut = new GetPrivateChatQueryHandler(
            _mapper,
            _chatRepository);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenChatDoesNotExist()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _chatRepository
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>())
            .Returns((Chat?)null);

        var query = new GetPrivateChatQuery(currentUserId, otherUserId);

        // Act
        var act = () => _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _chatRepository
            .Received(1)
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<ChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnChatDto_WhenChatExists()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        var chatDto = new ChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _mapper
            .Map<ChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDto);

        var query = new GetPrivateChatQuery(currentUserId, otherUserId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(chatDto);
        result.Id.Should().Be(chatId);

        await _chatRepository
            .Received(1)
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<ChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserIds_WhenGettingChat()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        var chatDto = new ChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _mapper
            .Map<ChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDto);

        var query = new GetPrivateChatQuery(currentUserId, otherUserId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapChatWithCorrectParameters_WhenChatExists()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        var chatDto = new ChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _mapper
            .Map<ChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDto);

        var query = new GetPrivateChatQuery(currentUserId, otherUserId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<ChatDto>(
                Arg.Is<Chat>(c => c.Id == chatId),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenMapperReturnsNull()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        var chat = new Chat
        {
            Id = chatId,
            CreatorId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId,
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _chatRepository
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _mapper
            .Map<ChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns((ChatDto?)null!);

        var query = new GetPrivateChatQuery(currentUserId, otherUserId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _chatRepository
            .Received(1)
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<ChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldNotMap_WhenChatNotFound()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _chatRepository
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId,
                Arg.Any<CancellationToken>())
            .Returns((Chat?)null);

        var query = new GetPrivateChatQuery(currentUserId, otherUserId);

        // Act
        var act = () => _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        _mapper
            .DidNotReceive()
            .Map<ChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectChatDto_WhenMultipleChatsExist()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId1 = Guid.NewGuid();
        var otherUserId2 = Guid.NewGuid();
        var chatId1 = Guid.NewGuid();
        var chatId2 = Guid.NewGuid();

        var chatWithUser1 = new Chat
        {
            Id = chatId1,
            CreatorId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = chatId1,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = otherUserId1,
                    ChatId = chatId1,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        var chatDto1 = new ChatDto
        {
            Id = chatId1
        };

        _chatRepository
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId1,
                Arg.Any<CancellationToken>())
            .Returns(chatWithUser1);

        _mapper
            .Map<ChatDto>(
                chatWithUser1,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(chatDto1);

        var query = new GetPrivateChatQuery(currentUserId, otherUserId1);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().Be(chatDto1);
        result.Id.Should().Be(chatId1);
        
        await _chatRepository
            .Received(1)
            .GetBetweenUsersAsync(
                currentUserId,
                otherUserId1,
                Arg.Any<CancellationToken>());
    }
}