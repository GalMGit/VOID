using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Commands.Delete;
using VOID.Application.UseCases.Messages.Events.Deleted;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using VOID.Shared.Contracts.Enums.Messages;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Messages.Commands.Delete;

public sealed class DeleteMessageCommandHandlerTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMessageBus _bus;
    private readonly IEncryptionService _encryptionService;
    private readonly IMapper _mapper;
    private readonly IChatRepository _chatRepository;

    private readonly DeleteMessageCommandHandler _sut;

    public DeleteMessageCommandHandlerTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _fileStorageService = Substitute.For<IFileStorageService>();
        _bus = Substitute.For<IMessageBus>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _mapper = Substitute.For<IMapper>();
        _chatRepository = Substitute.For<IChatRepository>();

        _sut = new DeleteMessageCommandHandler(
            _messageRepository,
            _fileStorageService,
            _bus,
            _encryptionService,
            _mapper,
            _chatRepository);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenMessageDoesNotExist()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _messageRepository
            .Received(1)
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var senderId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = senderId,
            ChatId = Guid.NewGuid(),
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();

        await _messageRepository
            .DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteTextMessage_WhenUserIsSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            MediaUrl = null,
            ThumbnailUrl = null,
            CreatedAt = DateTime.UtcNow
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _messageRepository
            .GetLastMessageAsync(chatId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .DeleteAsync(messageId, Arg.Any<CancellationToken>());

        await _fileStorageService
            .DidNotReceive()
            .DeleteMediaAsync(Arg.Any<string>(), Arg.Any<string>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<PrivateMessageDeletedEvent>(e =>
                    e.MessageId == messageId &&
                    e.UserId == userId &&
                    e.ChatId == chatId &&
                    e.RecipientId == recipientId &&
                    e.LastMessage == null));
    }

    [Fact]
    public async Task Handle_ShouldDeleteMedia_WhenMessageHasMedia()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Image,
            MediaUrl = "media/image.jpg",
            ThumbnailUrl = "media/thumb.jpg",
            CreatedAt = DateTime.UtcNow
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _messageRepository
            .GetLastMessageAsync(chatId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _fileStorageService
            .Received(1)
            .DeleteMediaAsync("media/image.jpg", "media/thumb.jpg");
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteMedia_WhenMessageIsText()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            MediaUrl = null,
            ThumbnailUrl = null,
            CreatedAt = DateTime.UtcNow
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _messageRepository
            .GetLastMessageAsync(chatId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _fileStorageService
            .DidNotReceive()
            .DeleteMediaAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldPublishGroupEvent_WhenMessageIsInGroup()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            GroupChatId = groupId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Group,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            MediaUrl = null,
            ThumbnailUrl = null,
            CreatedAt = DateTime.UtcNow
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupMessageDeletedEvent>(e =>
                    e.GroupId == groupId &&
                    e.MessageId == messageId &&
                    e.UserId == userId));

        await _chatRepository
            .DidNotReceive()
            .GetRecipientIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastMessage_WhenDeletingPrivateMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            CreatedAt = DateTime.UtcNow
        };

        var lastMessage = new Message
        {
            Id = Guid.NewGuid(),
            Text = "encrypted-last-message",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var lastMessageDto = new MessageDto
        {
            Id = lastMessage.Id,
            Text = "encrypted-last-message"
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _messageRepository
            .GetLastMessageAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(lastMessage);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        // Настраиваем mapper для возврата DTO
        _mapper
            .Map<MessageDto>(
                lastMessage,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(lastMessageDto);

        // Настраиваем decrypt
        _encryptionService
            .Decrypt("encrypted-last-message")
            .Returns("decrypted-last-message");

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .UpdateLastMessageAsync(
                chatId,
                lastMessage.Text,
                lastMessage.CreatedAt,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDecryptLastMessage_WhenMappingLastMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            CreatedAt = DateTime.UtcNow
        };

        var lastMessage = new Message
        {
            Id = Guid.NewGuid(),
            Text = "encrypted-last-message",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var lastMessageDto = new MessageDto
        {
            Id = lastMessage.Id,
            Text = "encrypted-last-message"
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _messageRepository
            .GetLastMessageAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(lastMessage);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _mapper
            .Map<MessageDto>(
                lastMessage,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(lastMessageDto);

        _encryptionService
            .Decrypt("encrypted-last-message")
            .Returns("decrypted-last-message");

        var command = new DeleteMessageCommand(messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _encryptionService
            .Received(1)
            .Decrypt("encrypted-last-message");

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<PrivateMessageDeletedEvent>(e =>
                    e.LastMessage != null &&
                    e.LastMessage.Text == "decrypted-last-message"));
    }
}