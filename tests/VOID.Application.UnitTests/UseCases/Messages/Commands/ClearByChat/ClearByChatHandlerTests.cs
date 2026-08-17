using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Commands.ClearByChat;
using VOID.Application.UseCases.Messages.Events.Cleared;
using VOID.Domain.Models.Chats;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Messages.Commands.ClearByChat;

public sealed class ClearByChatCommandHandlerTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IFileStorageService _storageService;
    private readonly IChatRepository _chatRepository;
    private readonly IMessageBus _bus;

    private readonly ClearByChatCommandHandler _sut;

    public ClearByChatCommandHandlerTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _storageService = Substitute.For<IFileStorageService>();
        _chatRepository = Substitute.For<IChatRepository>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new ClearByChatCommandHandler(
            _messageRepository,
            _storageService,
            _chatRepository,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenChatDoesNotExist()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns((Chat?)null);

        var command = new ClearByChatCommand(chatId, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _chatRepository
            .Received(1)
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .ClearMessagesByChatAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _storageService
            .DidNotReceive()
            .DeleteChatMessagesFolderAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
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
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        var command = new ClearByChatCommand(chatId, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();

        await _messageRepository
            .DidNotReceive()
            .ClearMessagesByChatAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _storageService
            .DidNotReceive()
            .DeleteChatMessagesFolderAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldClearMessages_WhenUserIsMember()
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

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        var command = new ClearByChatCommand(chatId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .ClearMessagesByChatAsync(chatId, Arg.Any<CancellationToken>());

        await _storageService
            .Received(1)
            .DeleteChatMessagesFolderAsync(chatId, Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .UpdateLastMessageAsync(
                chatId,
                null,
                null,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<MessagesByChatClearedEvent>(e =>
                    e.RecipientId == recipientId &&
                    e.UserId == userId &&
                    e.ChatId == chatId));
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenMessagesCleared()
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

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        var command = new ClearByChatCommand(chatId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<MessagesByChatClearedEvent>(e =>
                    e.RecipientId == recipientId &&
                    e.UserId == userId &&
                    e.ChatId == chatId));
    }

    [Fact]
    public async Task Handle_ShouldFindCorrectRecipient_WhenClearingMessages()
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

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        var command = new ClearByChatCommand(chatId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<MessagesByChatClearedEvent>(e =>
                    e.RecipientId == recipientId));
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastMessageToNull_WhenClearingMessages()
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

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        var command = new ClearByChatCommand(chatId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .UpdateLastMessageAsync(
                chatId,
                null,
                null,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteChatFolder_WhenClearingMessages()
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

        _chatRepository
            .GetByIdAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(chat);

        var command = new ClearByChatCommand(chatId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService
            .Received(1)
            .DeleteChatMessagesFolderAsync(
                chatId,
                Arg.Any<CancellationToken>());
    }
}