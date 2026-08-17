using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Queries.GetThumbnailMedia;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Messages.Queries.GetThumbnailMedia;

public sealed class GetThumbnailMediaQueryHandlerTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IGroupRepository _groupRepository;

    private readonly GetThumbnailMediaQueryHandler _sut;

    public GetThumbnailMediaQueryHandlerTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _chatRepository = Substitute.For<IChatRepository>();
        _fileStorageService = Substitute.For<IFileStorageService>();
        _groupRepository = Substitute.For<IGroupRepository>();

        _sut = new GetThumbnailMediaQueryHandler(
            _messageRepository,
            _chatRepository,
            _fileStorageService,
            _groupRepository);
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

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _messageRepository
            .Received(1)
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .IsMemberAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .IsMemberAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfPrivateChat()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MediaUrl = "media/test.jpg",
            ThumbnailUrl = "media/thumb.jpg",
            ContentType = "image/jpeg"
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();

        await _chatRepository
            .Received(1)
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>());

        _fileStorageService
            .DidNotReceive()
            .GetMessageMediaUrl(Arg.Any<string>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfGroup()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            GroupChatId = groupId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Group,
            MediaUrl = "media/test.jpg",
            ThumbnailUrl = "media/thumb.jpg",
            ContentType = "image/jpeg"
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _groupRepository
            .IsMemberAsync(groupId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();

        await _groupRepository
            .Received(1)
            .IsMemberAsync(groupId, userId, Arg.Any<CancellationToken>());

        _fileStorageService
            .DidNotReceive()
            .GetMessageMediaUrl(Arg.Any<string>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenMessageHasNoMedia()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MediaUrl = null,
            ThumbnailUrl = null,
            ContentType = null
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _fileStorageService
            .DidNotReceive()
            .GetMessageMediaUrl(Arg.Any<string>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldReturnThumbnailResult_WhenPrivateChatAndUserIsMember()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var thumbnailUrl = "media/thumb.jpg";
        var contentType = "image/jpeg";
        var expectedUrl = "https://example.com/media/thumb.jpg?expires=123";

        var message = new Message
        {
            Id = messageId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MediaUrl = "media/test.jpg",
            ThumbnailUrl = thumbnailUrl,
            ContentType = contentType
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _fileStorageService
            .GetMessageMediaUrl(thumbnailUrl, TimeSpan.FromMinutes(10))
            .Returns(expectedUrl);

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ThumbnailUrl.Should().Be(expectedUrl);
        result.ContentType.Should().Be(contentType);

        _fileStorageService
            .Received(1)
            .GetMessageMediaUrl(thumbnailUrl, TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task Handle_ShouldReturnThumbnailResult_WhenGroupChatAndUserIsMember()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var thumbnailUrl = "media/video-thumb.jpg";
        var contentType = "video/mp4";
        var expectedUrl = "https://example.com/media/video-thumb.jpg?expires=456";

        var message = new Message
        {
            Id = messageId,
            GroupChatId = groupId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Group,
            MediaUrl = "media/video.mp4",
            ThumbnailUrl = thumbnailUrl,
            ContentType = contentType
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _groupRepository
            .IsMemberAsync(groupId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _fileStorageService
            .GetMessageMediaUrl(thumbnailUrl, TimeSpan.FromMinutes(10))
            .Returns(expectedUrl);

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ThumbnailUrl.Should().Be(expectedUrl);
        result.ContentType.Should().Be(contentType);

        _fileStorageService
            .Received(1)
            .GetMessageMediaUrl(thumbnailUrl, TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task Handle_ShouldUseThumbnailUrl_WhenGettingThumbnail()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var mediaUrl = "media/test.jpg";
        var thumbnailUrl = "media/thumbnail-specific.jpg";
        var contentType = "image/jpeg";
        var expectedUrl = "https://example.com/media/thumbnail-specific.jpg";

        var message = new Message
        {
            Id = messageId,
            ChatId = chatId,
            ChatType = VOID.Domain.Enums.Types.Chat.ChatType.Private,
            MediaUrl = mediaUrl,
            ThumbnailUrl = thumbnailUrl,
            ContentType = contentType
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _chatRepository
            .IsMemberAsync(chatId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _fileStorageService
            .GetMessageMediaUrl(thumbnailUrl, TimeSpan.FromMinutes(10))
            .Returns(expectedUrl);

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.ThumbnailUrl.Should().Be(expectedUrl);

        _fileStorageService
            .Received(1)
            .GetMessageMediaUrl(
                thumbnailUrl,
                TimeSpan.FromMinutes(10));

        // Проверяем, что используется thumbnailUrl, а не mediaUrl
        _fileStorageService
            .DidNotReceive()
            .GetMessageMediaUrl(
                mediaUrl,
                Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenChatTypeIsUnknown()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            ChatType = (VOID.Domain.Enums.Types.Chat.ChatType)999,
            MediaUrl = "media/test.jpg",
            ThumbnailUrl = "media/thumb.jpg",
            ContentType = "image/jpeg"
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        var query = new GetThumbnailMediaQuery(userId, messageId);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}