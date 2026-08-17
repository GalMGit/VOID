using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Chats.Queries.GetById;
using VOID.Domain.Models.Chats;
using VOID.Shared.Contracts.DTOs.Chats;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Chats.Queries.GetById;

public sealed class GetChatByIdQueryHandlerTests
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    private readonly GetChatByIdQueryHandler _sut;

    public GetChatByIdQueryHandlerTests()
    {
        _chatRepository = Substitute.For<IChatRepository>();
        _messageRepository = Substitute.For<IMessageRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetChatByIdQueryHandler(
            _chatRepository,
            _messageRepository,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenChatDoesNotExist()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _chatRepository
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns((Chat?)null);

        var query = new GetChatByIdQuery(chatId, userId);

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
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .GetTotalCountByChatAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<FullChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfChat()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _chatRepository
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        var query = new GetChatByIdQuery(chatId, userId);

        // Act
        var act = () => _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ForbiddenException>();

        await _chatRepository
            .Received(1)
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .GetTotalCountByChatAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<FullChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFullChatDto_WhenUserIsMember()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var messageCount = 10;

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

        var fullChatDto = new FullChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetTotalCountByChatAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(messageCount);

        _mapper
            .Map<FullChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(fullChatDto);

        var query = new GetChatByIdQuery(chatId, userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(fullChatDto);
        result.MessageCount.Should().Be(messageCount);

        await _chatRepository
            .Received(1)
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _messageRepository
            .Received(1)
            .GetTotalCountByChatAsync(
                chatId,
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<FullChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldSetMessageCount_WhenReturningChat()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var expectedMessageCount = 25;

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

        var fullChatDto = new FullChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetTotalCountByChatAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(expectedMessageCount);

        _mapper
            .Map<FullChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(fullChatDto);

        var query = new GetChatByIdQuery(chatId, userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.MessageCount.Should().Be(expectedMessageCount);
    }

    [Fact]
    public async Task Handle_ShouldCallMapperWithCorrectParameters_WhenReturningChat()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

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

        var fullChatDto = new FullChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetTotalCountByChatAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _mapper
            .Map<FullChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(fullChatDto);

        var query = new GetChatByIdQuery(chatId, userId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<FullChatDto>(
                Arg.Is<Chat>(c => c.Id == chatId),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldGetMessageCount_WhenUserIsMember()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

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

        var fullChatDto = new FullChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetTotalCountByChatAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(5);

        _mapper
            .Map<FullChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(fullChatDto);

        var query = new GetChatByIdQuery(chatId, userId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .GetTotalCountByChatAsync(
                chatId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldWorkWithZeroMessageCount_WhenChatHasNoMessages()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

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

        var fullChatDto = new FullChatDto
        {
            Id = chatId
        };

        _chatRepository
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _messageRepository
            .GetTotalCountByChatAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _mapper
            .Map<FullChatDto>(
                chat,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(fullChatDto);

        var query = new GetChatByIdQuery(chatId, userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageCount.Should().Be(0);
    }
}