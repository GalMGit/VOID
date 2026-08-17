using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Chats.Commands.Delete;
using VOID.Application.UseCases.Chats.Events.Deleted;
using VOID.Domain.Models.Chats;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Chats.Commands.Delete;

public sealed class DeleteChatCommandHandlerTests
{
    private readonly IChatRepository _chatRepository;
    private readonly IFileStorageService _storageService;
    private readonly IMessageBus _bus;

    private readonly DeleteChatCommandHandler _sut;

    public DeleteChatCommandHandlerTests()
    {
        _chatRepository = Substitute.For<IChatRepository>();
        _storageService = Substitute.For<IFileStorageService>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new DeleteChatCommandHandler(
            _chatRepository,
            _storageService,
            _bus);
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

        var command = new DeleteChatCommand(chatId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
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

        await _chatRepository
            .DidNotReceive()
            .GetRecipientIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .HasMediaAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _storageService
            .DidNotReceive()
            .DeleteChatMessagesFolderAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .DeleteAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<ChatDeletedEvent>());
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

        var command = new DeleteChatCommand(chatId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
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

        await _chatRepository
            .DidNotReceive()
            .GetRecipientIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .HasMediaAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _storageService
            .DidNotReceive()
            .DeleteChatMessagesFolderAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .DeleteAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<ChatDeletedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteChatWithoutMedia_WhenChatHasNoMedia()
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
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _chatRepository
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _chatRepository
            .HasMediaAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteChatCommand(chatId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .HasMediaAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _storageService
            .DidNotReceive()
            .DeleteChatMessagesFolderAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .DeleteAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<ChatDeletedEvent>(e =>
                    e.ChatId == chatId &&
                    e.RecipientId == recipientId));
    }

    [Fact]
    public async Task Handle_ShouldDeleteChatWithMedia_WhenChatHasMedia()
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
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _chatRepository
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _chatRepository
            .HasMediaAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteChatCommand(chatId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .HasMediaAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _storageService
            .Received(1)
            .DeleteChatMessagesFolderAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .DeleteAsync(
                chatId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<ChatDeletedEvent>(e =>
                    e.ChatId == chatId &&
                    e.RecipientId == recipientId));
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenChatDeleted()
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
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _chatRepository
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _chatRepository
            .HasMediaAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteChatCommand(chatId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<ChatDeletedEvent>(e =>
                    e.ChatId == chatId &&
                    e.RecipientId == recipientId));
    }

    [Fact]
    public async Task Handle_ShouldGetRecipientId_WhenDeletingChat()
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
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _chatRepository
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _chatRepository
            .HasMediaAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteChatCommand(chatId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteChat_WhenUserIsMember()
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
            .GetByIdAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(chat);

        _chatRepository
            .GetRecipientIdAsync(
                userId,
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _chatRepository
            .HasMediaAsync(
                chatId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteChatCommand(chatId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .DeleteAsync(
                chatId,
                Arg.Any<CancellationToken>());
    }
}