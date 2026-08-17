using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Commands.Update;
using VOID.Application.UseCases.Messages.Events.Updated;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Messages.Commands.Update;

public sealed class UpdateMessageCommandHandlerTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMapper _mapper;
    private readonly IEncryptionService _encryptionService;
    private readonly IMessageBus _bus;

    private readonly UpdateMessageCommandHandler _sut;

    public UpdateMessageCommandHandlerTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _chatRepository = Substitute.For<IChatRepository>();
        _mapper = Substitute.For<IMapper>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new UpdateMessageCommandHandler(
            _messageRepository,
            _chatRepository,
            _mapper,
            _encryptionService,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenMessageDoesNotExist()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new UpdateMessageDto { Text = "Updated text" };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        var command = new UpdateMessageCommand(dto, messageId, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _messageRepository
            .Received(1)
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var dto = new UpdateMessageDto { Text = "Updated text" };

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

        var command = new UpdateMessageCommand(dto, messageId, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();

        await _messageRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdatePrivateMessage_WhenUserIsSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto = new UpdateMessageDto { Text = "Updated text" };

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "Original text",
            IsEdited = false
        };

        var updatedMessage = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "encrypted-updated-text",
            IsEdited = true
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            Text = "decrypted-updated-text",
            SenderId = userId,
            IsEdited = true
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _encryptionService
            .Encrypt("Updated text")
            .Returns("encrypted-updated-text");

        _messageRepository
            .UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(updatedMessage);

        _encryptionService
            .Decrypt("encrypted-updated-text")
            .Returns("decrypted-updated-text");

        _mapper
            .Map<MessageDto>(
                updatedMessage,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        var command = new UpdateMessageCommand(dto, messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<Message>(m =>
                    m.Id == messageId &&
                    m.Text == "encrypted-updated-text" &&
                    m.IsEdited == true),
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<PrivateMessageUpdatedEvent>(e =>
                    e.Message == messageDto &&
                    e.ChatId == chatId &&
                    e.RecipientId == recipientId &&
                    e.UserId == userId));
    }

    [Fact]
    public async Task Handle_ShouldUpdateGroupMessage_WhenUserIsSender()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var dto = new UpdateMessageDto { Text = "Updated group text" };

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            GroupChatId = groupId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Group,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "Original text",
            IsEdited = false
        };

        var updatedMessage = new Message
        {
            Id = messageId,
            SenderId = userId,
            GroupChatId = groupId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Group,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "encrypted-updated-text",
            IsEdited = true
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            Text = "decrypted-updated-text",
            SenderId = userId,
            IsEdited = true
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _encryptionService
            .Encrypt("Updated group text")
            .Returns("encrypted-updated-text");

        _messageRepository
            .UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(updatedMessage);

        _encryptionService
            .Decrypt("encrypted-updated-text")
            .Returns("decrypted-updated-text");

        _mapper
            .Map<MessageDto>(
                updatedMessage,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var command = new UpdateMessageCommand(dto, messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupMessageUpdatedEvent>(e =>
                    e.Message == messageDto &&
                    e.GroupId == groupId &&
                    e.UserId == userId));

        await _chatRepository
            .DidNotReceive()
            .GetRecipientIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetIsEditedToTrue_WhenTextIsUpdated()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto = new UpdateMessageDto { Text = "New text" };

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "Old text",
            IsEdited = false
        };

        var updatedMessage = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "encrypted-new-text",
            IsEdited = true
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            Text = "decrypted-new-text",
            SenderId = userId,
            IsEdited = true
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _encryptionService
            .Encrypt("New text")
            .Returns("encrypted-new-text");

        _messageRepository
            .UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(updatedMessage);

        _encryptionService
            .Decrypt("encrypted-new-text")
            .Returns("decrypted-new-text");

        _mapper
            .Map<MessageDto>(
                updatedMessage,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        var command = new UpdateMessageCommand(dto, messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<Message>(m => m.IsEdited == true),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldEncryptText_WhenUpdatingMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto = new UpdateMessageDto { Text = "Secret text" };

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "Old text",
            IsEdited = false
        };

        var updatedMessage = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "encrypted-secret",
            IsEdited = true
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            Text = "decrypted-secret",
            SenderId = userId,
            IsEdited = true
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _encryptionService
            .Encrypt("Secret text")
            .Returns("encrypted-secret");

        _messageRepository
            .UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(updatedMessage);

        _encryptionService
            .Decrypt("encrypted-secret")
            .Returns("decrypted-secret");

        _mapper
            .Map<MessageDto>(
                updatedMessage,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        var command = new UpdateMessageCommand(dto, messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _encryptionService
            .Received(1)
            .Encrypt("Secret text");

        _encryptionService
            .Received(1)
            .Decrypt("encrypted-secret");
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateText_WhenDtoTextIsNullOrEmpty()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto = new UpdateMessageDto { Text = null };

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "Original text",
            IsEdited = false
        };

        var updatedMessage = new Message
        {
            Id = messageId,
            SenderId = userId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            Text = "encrypted-original",
            IsEdited = false
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            Text = "decrypted-original",
            SenderId = userId,
            IsEdited = false
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _encryptionService
            .Encrypt("Original text")
            .Returns("encrypted-original");

        _messageRepository
            .UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(updatedMessage);

        _encryptionService
            .Decrypt("encrypted-original")
            .Returns("decrypted-original");

        _mapper
            .Map<MessageDto>(
                updatedMessage,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        var command = new UpdateMessageCommand(dto, messageId, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<Message>(m => m.IsEdited == false),
                Arg.Any<CancellationToken>());
    }
}