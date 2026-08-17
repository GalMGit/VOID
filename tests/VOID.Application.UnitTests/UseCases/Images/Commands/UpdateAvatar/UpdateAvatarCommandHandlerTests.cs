using System.Text;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Images.Commands.UpdateAvatar;
using VOID.Application.UseCases.Images.Events;
using VOID.Shared.Contracts.DTOs.Users.Avatars;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Images.Commands.UpdateAvatar;

public sealed class UpdateAvatarCommandHandlerTests
{
    private readonly IImageRepository _imageRepository;
    private readonly IFileStorageService _storageService;
    private readonly IMediaUrlService _mediaUrlService;
    private readonly IMessageBus _bus;

    private readonly UpdateAvatarCommandHandler _sut;

    public UpdateAvatarCommandHandlerTests()
    {
        _imageRepository = Substitute.For<IImageRepository>();
        _storageService = Substitute.For<IFileStorageService>();
        _mediaUrlService = Substitute.For<IMediaUrlService>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new UpdateAvatarCommandHandler(
            _imageRepository,
            _storageService,
            _mediaUrlService,
            _bus);
    }

    [Fact]
    public async Task HandleAsync_ShouldUploadNewAvatar_WhenMediaProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "avatar.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "avatars/user-avatar.jpg",
            null,
            "image/jpeg");

        var expectedUrl = "https://example.com/avatars/user-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _storageService
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns(expectedUrl);

        var command = new UpdateAvatarCommand(userId, media);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().Be(expectedUrl);

        await _imageRepository
            .Received(1)
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>());

        await _storageService
            .Received(1)
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>());

        await _imageRepository
            .Received(1)
            .UpdateAvatarAsync(
                uploadResult.RelativePath,
                userId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<AvatarUpdatedEvent>(e =>
                    e.UserId == userId &&
                    e.AvatarUrl == expectedUrl));
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteOldAvatar_WhenOldAvatarExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldAvatarUrl = "avatars/old-avatar.jpg";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "new-avatar.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "avatars/new-avatar.jpg",
            null,
            "image/jpeg");

        var expectedUrl = "https://example.com/avatars/new-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(oldAvatarUrl);

        _storageService
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns(expectedUrl);

        var command = new UpdateAvatarCommand(userId, media);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().Be(expectedUrl);

        await _storageService
            .Received(1)
            .DeleteAvatarAsync(oldAvatarUrl, Arg.Any<CancellationToken>());

        await _storageService
            .Received(1)
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldNotUpload_WhenMediaIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUrl = "https://example.com/avatars/default.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _mediaUrlService
            .GetAvatarUrl(null)
            .Returns(expectedUrl);

        var command = new UpdateAvatarCommand(userId, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().Be(expectedUrl);

        await _storageService
            .DidNotReceive()
            .UploadAvatarAsync(
                Arg.Any<UploadFile>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _imageRepository
            .Received(1)
            .UpdateAvatarAsync(
                null,
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldNotDeleteOldAvatar_WhenOldAvatarDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "avatar.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "avatars/user-avatar.jpg",
            null,
            "image/jpeg");

        var expectedUrl = "https://example.com/avatars/user-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _storageService
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns(expectedUrl);

        var command = new UpdateAvatarCommand(userId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService
            .DidNotReceive()
            .DeleteAvatarAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishEventWithCorrectData_WhenAvatarUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "avatar.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "avatars/user-avatar.jpg",
            null,
            "image/jpeg");

        var expectedUrl = "https://example.com/avatars/user-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _storageService
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns(expectedUrl);

        var command = new UpdateAvatarCommand(userId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<AvatarUpdatedEvent>(e =>
                    e.UserId == userId &&
                    e.AvatarUrl == expectedUrl));
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateAvatarWithCorrectPath_WhenUploading()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "avatar.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "avatars/specific-path.jpg",
            null,
            "image/jpeg");

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _storageService
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns("https://example.com/avatars/specific-path.jpg");

        var command = new UpdateAvatarCommand(userId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _imageRepository
            .Received(1)
            .UpdateAvatarAsync(
                "avatars/specific-path.jpg",
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAvatarDtoWithCorrectUrl_WhenUpdatingAvatar()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldAvatarUrl = "avatars/old.jpg";
        var expectedUrl = "https://example.com/avatars/default.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(oldAvatarUrl);

        _mediaUrlService
            .GetAvatarUrl(null)
            .Returns(expectedUrl);

        var command = new UpdateAvatarCommand(userId, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().Be(expectedUrl);

        await _storageService
            .Received(1)
            .DeleteAvatarAsync(oldAvatarUrl, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteOldAvatarAndUploadNew_WhenBothExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldAvatarUrl = "avatars/old-avatar.jpg";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image"));
        var media = new UploadFile
        {
            FileName = "new-avatar.jpg",
            ContentType = "image/jpeg",
            Length = stream.Length,
            Stream = stream
        };

        var uploadResult = new FileUploadResult(
            "avatars/new-avatar.jpg",
            null,
            "image/jpeg");

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(oldAvatarUrl);

        _storageService
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        _mediaUrlService
            .GetAvatarUrl(uploadResult.RelativePath)
            .Returns("https://example.com/avatars/new-avatar.jpg");

        var command = new UpdateAvatarCommand(userId, media);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _storageService
            .Received(1)
            .DeleteAvatarAsync(oldAvatarUrl, Arg.Any<CancellationToken>());

        await _storageService
            .Received(1)
            .UploadAvatarAsync(media, userId, Arg.Any<CancellationToken>());
    }
}