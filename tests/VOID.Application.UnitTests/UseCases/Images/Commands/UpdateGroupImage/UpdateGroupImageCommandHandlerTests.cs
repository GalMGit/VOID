using System.Text;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Images.Commands.UpdateGroupImage;
using VOID.Application.UseCases.Images.Events;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Images.Commands.UpdateGroupImage;

public sealed class UpdateGroupImageCommandHandlerTests
{
    private readonly IImageRepository _imageRepository;
    private readonly IFileStorageService _storageService;
    private readonly IMediaUrlService _mediaUrlService;
    private readonly IMessageBus _bus;
    private readonly IMapper _mapper;

    private readonly UpdateGroupImageCommandHandler _sut;

    public UpdateGroupImageCommandHandlerTests()
    {
        _imageRepository = Substitute.For<IImageRepository>();
        _storageService = Substitute.For<IFileStorageService>();
        _mediaUrlService = Substitute.For<IMediaUrlService>();
        _bus = Substitute.For<IMessageBus>();
        _mapper = Substitute.For<IMapper>();

        _sut = new UpdateGroupImageCommandHandler(
            _imageRepository,
            _storageService,
            _mapper,
            _mediaUrlService,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldUploadGroupImage_WhenMediaProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "group-image.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "groups/group-image.jpg",
            null,
            "image/jpeg");

        var expectedUrl = "https://example.com/groups/group-image.jpg";

        _storageService
            .UploadGroupImageAsync(media, groupId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns(expectedUrl);

        var command = new UpdateGroupImageCommand(userId, groupId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService
            .Received(1)
            .UploadGroupImageAsync(media, groupId, Arg.Any<CancellationToken>());

        await _imageRepository
            .Received(1)
            .UpdateGroupImageAsync(
                uploadResult.RelativePath,
                groupId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupImageUpdatedEvent>(e =>
                    e.UserId == userId &&
                    e.GroupId == groupId &&
                    e.ImageUrl == expectedUrl));
    }

    [Fact]
    public async Task Handle_ShouldNotUpload_WhenMediaIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var expectedUrl = "https://example.com/groups/default.jpg";

        _mediaUrlService
            .GetAvatarUrl(null)
            .Returns(expectedUrl);

        var command = new UpdateGroupImageCommand(userId, groupId, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService
            .DidNotReceive()
            .UploadGroupImageAsync(
                Arg.Any<UploadFile>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _imageRepository
            .Received(1)
            .UpdateGroupImageAsync(
                null,
                groupId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupImageUpdatedEvent>(e =>
                    e.ImageUrl == expectedUrl));
    }

    [Fact]
    public async Task Handle_ShouldUpdateGroupImageWithCorrectPath_WhenUploading()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "group-image.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "groups/specific-path.jpg",
            null,
            "image/jpeg");

        _storageService
            .UploadGroupImageAsync(media, groupId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns("https://example.com/groups/specific-path.jpg");

        var command = new UpdateGroupImageCommand(userId, groupId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _imageRepository
            .Received(1)
            .UpdateGroupImageAsync(
                "groups/specific-path.jpg",
                groupId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenImageUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "group-image.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "groups/group-image.jpg",
            null,
            "image/jpeg");

        var expectedUrl = "https://example.com/groups/group-image.jpg";

        _storageService
            .UploadGroupImageAsync(media, groupId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns(expectedUrl);

        var command = new UpdateGroupImageCommand(userId, groupId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupImageUpdatedEvent>(e =>
                    e.UserId == userId &&
                    e.GroupId == groupId &&
                    e.ImageUrl == expectedUrl));
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectGroupId_WhenUploadingImage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var differentGroupId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "group-image.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "groups/group-image.jpg",
            null,
            "image/jpeg");

        _storageService
            .UploadGroupImageAsync(media, groupId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns("https://example.com/groups/group-image.jpg");

        var command = new UpdateGroupImageCommand(userId, groupId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService
            .Received(1)
            .UploadGroupImageAsync(
                media,
                groupId,
                Arg.Any<CancellationToken>());

        await _storageService
            .DidNotReceive()
            .UploadGroupImageAsync(
                media,
                differentGroupId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectUserId_WhenPublishingEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "group-image.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "groups/group-image.jpg",
            null,
            "image/jpeg");

        _storageService
            .UploadGroupImageAsync(media, groupId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns("https://example.com/groups/group-image.jpg");

        var command = new UpdateGroupImageCommand(userId, groupId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupImageUpdatedEvent>(e =>
                    e.UserId == userId));

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Is<GroupImageUpdatedEvent>(e =>
                    e.UserId == differentUserId));
    }

    [Fact]
    public async Task Handle_ShouldGenerateCorrectUrl_WhenUpdatingImage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "group-image.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "groups/group-image.jpg",
            null,
            "image/jpeg");

        var expectedUrl = "https://example.com/groups/group-image.jpg";

        _storageService
            .UploadGroupImageAsync(media, groupId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns(expectedUrl);

        var command = new UpdateGroupImageCommand(userId, groupId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mediaUrlService
            .Received(1)
            .GetAvatarUrl("groups/group-image.jpg");
    }

    [Fact]
    public async Task Handle_ShouldGenerateDefaultUrl_WhenMediaIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var expectedUrl = "https://example.com/groups/default.jpg";

        _mediaUrlService
            .GetAvatarUrl(null)
            .Returns(expectedUrl);

        var command = new UpdateGroupImageCommand(userId, groupId, null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mediaUrlService
            .Received(1)
            .GetAvatarUrl(null);
    }
}