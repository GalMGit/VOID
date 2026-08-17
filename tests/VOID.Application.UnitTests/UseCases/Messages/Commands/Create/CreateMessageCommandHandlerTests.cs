using System.Text;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Commands.Create;
using VOID.Application.UseCases.Messages.Events.Created;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using VOID.Shared.Contracts.Enums.Chats;
using VOID.Shared.Contracts.Enums.Messages;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Messages.Create;

public sealed class CreateMessageCommandHandlerTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IFileStorageService _storageService;
    private readonly IEncryptionService _encryptionService;
    private readonly IMessageBus _bus;
    private readonly IMapper _mapper;

    private readonly CreateMessageCommandHandler _sut;

    public CreateMessageCommandHandlerTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _chatRepository = Substitute.For<IChatRepository>();
        _groupRepository = Substitute.For<IGroupRepository>();
        _storageService = Substitute.For<IFileStorageService>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _bus = Substitute.For<IMessageBus>();
        _mapper = Substitute.For<IMapper>();

        _sut = new CreateMessageCommandHandler(
            _messageRepository,
            _chatRepository,
            _groupRepository,
            _storageService,
            _encryptionService,
            _bus,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenPrivateChatDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = "Hello",
            ParentId = chatId,
            MessageType = MessageType.Text,
            ChatType = ChatType.Private
        };

        _chatRepository
            .ExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _chatRepository.Received(1).ExistsAsync(chatId, Arg.Any<CancellationToken>());
        await _messageRepository.DidNotReceive().CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfPrivateChat()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = "Hello",
            ParentId = chatId,
            MessageType = MessageType.Text,
            ChatType = ChatType.Private
        };

        _chatRepository
            .ExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(true);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();

        await _messageRepository.DidNotReceive().CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = "Hello",
            ParentId = groupId,
            MessageType = MessageType.Text,
            ChatType = ChatType.Group
        };

        _groupRepository
            .ExistsAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _messageRepository.DidNotReceive().CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = "Hello",
            ParentId = groupId,
            MessageType = MessageType.Text,
            ChatType = ChatType.Group
        };

        _groupRepository
            .ExistsAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(groupId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();

        await _messageRepository.DidNotReceive().CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateTextMessage_WhenPrivateChatAndValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = "Hello World",
            ParentId = chatId,
            MessageType = MessageType.Text,
            ChatType = ChatType.Private
        };

        var createdMessage = new Message
        {
            Id = messageId,
            Text = "encrypted-text",
            SenderId = userId,
            ChatId = chatId,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            Text = "Hello World",
            SenderId = userId,
            ChatType = ChatType.Private,
            MessageType = MessageType.Text,
            ParentId = chatId
        };

        _chatRepository
            .ExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(true);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _encryptionService
            .Encrypt(Arg.Any<string>())
            .Returns("encrypted-text", "encrypted-preview");

        _messageRepository
            .CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(createdMessage);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _mapper
            .Map<MessageDto>(createdMessage, Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(messageId);
        result.Text.Should().Be("Hello World");
        
        await _messageRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<Message>(m =>
                    m.SenderId == userId &&
                    m.ChatId == chatId &&
                    m.Text != null),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .UpdateLastMessageAsync(
                chatId,
                "encrypted-preview",
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<PrivateMessageCreatedEvent>(e =>
                    e.Message == messageDto &&
                    e.RecipientId == recipientId));
    }


    [Fact]
    public async Task Handle_ShouldCreateMessageWithMedia_WhenMediaProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = null,
            ParentId = chatId,
            MessageType = MessageType.Image,
            ChatType = ChatType.Private
        };

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var media = new UploadFile
        {
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "media/test.jpg",
            "media/thumb.jpg",
            "image/jpeg");

        var createdMessage = new Message
        {
            Id = Guid.NewGuid(),
            MediaUrl = "media/test.jpg",
            ThumbnailUrl = "media/thumb.jpg",
            ContentType = "image/jpeg",
            SenderId = userId,
            ChatId = chatId,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Image,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = createdMessage.Id,
            SenderId = userId,
            ChatType = ChatType.Private,
            MessageType = MessageType.Image,
            ParentId = chatId,
            MediaUrl = "media/test.jpg"
        };

        _chatRepository
            .ExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(true);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _storageService
            .UploadMessageMediaAsync(media, chatId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _messageRepository
            .CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(createdMessage);

        _encryptionService
            .Encrypt("📷 Фото")
            .Returns("encrypted-preview");

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(recipientId);

        _mapper
            .Map<MessageDto>(createdMessage, Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var command = new CreateMessageCommand(dto, userId, media);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MediaUrl.Should().Be("media/test.jpg");

        await _storageService
            .Received(1)
            .UploadMessageMediaAsync(media, chatId, Arg.Any<CancellationToken>());

        await _messageRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<Message>(m =>
                    m.MediaUrl == "media/test.jpg" &&
                    m.ThumbnailUrl == "media/thumb.jpg" &&
                    m.ContentType == "image/jpeg"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateGroupMessage_WhenChatTypeIsGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = "Group message",
            ParentId = groupId,
            MessageType = MessageType.Text,
            ChatType = ChatType.Group
        };

        var createdMessage = new Message
        {
            Id = Guid.NewGuid(),
            Text = "encrypted-group-message",
            SenderId = userId,
            GroupChatId = groupId,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Group,
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = createdMessage.Id,
            Text = "Group message",
            SenderId = userId,
            ChatType = ChatType.Group,
            MessageType = MessageType.Text,
            ParentId = groupId
        };

        _groupRepository
            .ExistsAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(groupId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _encryptionService
            .Encrypt("Group message")
            .Returns("encrypted-group-message");

        _messageRepository
            .CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(createdMessage);

        _mapper
            .Map<MessageDto>(createdMessage, Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        await _messageRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<Message>(m =>
                    m.GroupChatId == groupId &&
                    m.ChatId == null &&
                    m.ChatType == VOID.Domain.Enums.Types.Chat.ChatType.Group),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .UpdateLastMessageAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupMessageCreatedEvent>(e =>
                    e.Message == messageDto));
    }

    [Fact]
    public async Task Handle_ShouldEncryptText_WhenCreatingMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = "Secret message",
            ParentId = chatId,
            MessageType = MessageType.Text,
            ChatType = ChatType.Private
        };

        var createdMessage = new Message
        {
            Id = Guid.NewGuid(),
            Text = "encrypted-secret",
            SenderId = userId,
            ChatId = chatId,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Text,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = createdMessage.Id,
            Text = "Secret message",
            SenderId = userId,
            ChatType = ChatType.Private,
            MessageType = MessageType.Text,
            ParentId = chatId
        };

        _chatRepository
            .ExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(true);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _encryptionService
            .Encrypt(Arg.Any<string>())
            .Returns("encrypted-secret", "encrypted-preview");

        _messageRepository
            .CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(createdMessage);

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        _mapper
            .Map<MessageDto>(createdMessage, Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        // Encrypt вызывается дважды: для текста сообщения и для preview
        _encryptionService
            .Received(2)
            .Encrypt(Arg.Any<string>());

        await _messageRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<Message>(m => m.Text == "encrypted-secret"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotEncrypt_WhenTextIsNullOrEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var dto = new CreateMessageDto
        {
            Text = null,
            ParentId = chatId,
            MessageType = MessageType.Image,
            ChatType = ChatType.Private
        };

        var createdMessage = new Message
        {
            Id = Guid.NewGuid(),
            Text = null,
            SenderId = userId,
            ChatId = chatId,
            MessageType = VOID.Domain.Enums.Types.Message.MessageType.Image,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = createdMessage.Id,
            SenderId = userId,
            ChatType = ChatType.Private,
            MessageType = MessageType.Image,
            ParentId = chatId
        };

        _chatRepository
            .ExistsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(true);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _messageRepository
            .CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(createdMessage);

        _encryptionService
            .Encrypt("📷 Фото")
            .Returns("encrypted-preview");

        _chatRepository
            .GetRecipientIdAsync(userId, chatId, Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        _mapper
            .Map<MessageDto>(createdMessage, Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var command = new CreateMessageCommand(dto, userId, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _encryptionService
            .DidNotReceive()
            .Encrypt(Arg.Is<string>(s => s == null || s == ""));
    }
}