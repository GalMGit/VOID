using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Images.Query.GetAvatar;
using VOID.Shared.Contracts.DTOs.Users.Avatars;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Images.Query.GetAvatar;

public sealed class GetAvatarQueryHandlerTests
{
    private readonly IImageRepository _imageRepository;
    private readonly IMediaUrlService _mediaUrlService;

    private readonly GetAvatarQueryHandler _sut;

    public GetAvatarQueryHandlerTests()
    {
        _imageRepository = Substitute.For<IImageRepository>();
        _mediaUrlService = Substitute.For<IMediaUrlService>();

        _sut = new GetAvatarQueryHandler(
            _imageRepository,
            _mediaUrlService);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAvatarDto_WhenAvatarExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var avatarPath = "avatars/user-avatar.jpg";
        var expectedUrl = "https://example.com/avatars/user-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(avatarPath);

        _mediaUrlService
            .GetAvatarUrl(avatarPath)
            .Returns(expectedUrl);

        var query = new GetAvatarQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().Be(expectedUrl);

        await _imageRepository
            .Received(1)
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>());

        _mediaUrlService
            .Received(1)
            .GetAvatarUrl(avatarPath);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNullAvatarUrl_WhenAvatarDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _mediaUrlService
            .GetAvatarUrl(null)
            .Returns((string?)null!);

        var query = new GetAvatarQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().BeNull();

        await _imageRepository
            .Received(1)
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>());

        _mediaUrlService
            .Received(1)
            .GetAvatarUrl(null);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectUserId_WhenGettingAvatar()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var avatarPath = "avatars/user-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(avatarPath);

        _mediaUrlService
            .GetAvatarUrl(avatarPath)
            .Returns("https://example.com/avatar.jpg");

        var query = new GetAvatarQuery(userId);

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _imageRepository
            .Received(1)
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>());

        await _imageRepository
            .DidNotReceive()
            .GetAvatarUrlByUserAsync(differentUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldUseCorrectAvatarPath_WhenGettingUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var avatarPath = "avatars/specific-avatar.jpg";
        var expectedUrl = "https://example.com/avatars/specific-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(avatarPath);

        _mediaUrlService
            .GetAvatarUrl(avatarPath)
            .Returns(expectedUrl);

        var query = new GetAvatarQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.AvatarUrl.Should().Be(expectedUrl);

        _mediaUrlService
            .Received(1)
            .GetAvatarUrl(avatarPath);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAvatarDtoWithCorrectProperties_WhenAvatarExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var avatarPath = "avatars/user-avatar.jpg";
        var expectedUrl = "https://example.com/avatars/user-avatar.jpg";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(avatarPath);

        _mediaUrlService
            .GetAvatarUrl(avatarPath)
            .Returns(expectedUrl);

        var query = new GetAvatarQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeOfType<AvatarDto>();
        result.AvatarUrl.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleEmptyAvatarPath_WhenAvatarPathIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var avatarPath = string.Empty;

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(avatarPath);

        _mediaUrlService
            .GetAvatarUrl(avatarPath)
            .Returns((string?)null!);

        var query = new GetAvatarQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().BeNull();

        _mediaUrlService
            .Received(1)
            .GetAvatarUrl(avatarPath);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleWhitespaceAvatarPath_WhenAvatarPathIsWhitespace()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var avatarPath = "   ";

        _imageRepository
            .GetAvatarUrlByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(avatarPath);

        _mediaUrlService
            .GetAvatarUrl(avatarPath)
            .Returns((string?)null!);

        var query = new GetAvatarQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().BeNull();

        _mediaUrlService
            .Received(1)
            .GetAvatarUrl(avatarPath);
    }
}