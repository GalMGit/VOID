using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Auth.Commands.Refresh;
using VOID.Domain.Enums.Roles.App;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Token;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtProvider _jwtProvider;

    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _jwtProvider = Substitute.For<IJwtProvider>();

        _sut = new RefreshTokenCommandHandler(
            _refreshTokenRepository,
            _jwtProvider);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenTokenDoesNotExist()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "non-existent-token"
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var command = new RefreshTokenCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ForbiddenException>();

        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        _jwtProvider
            .DidNotReceive()
            .GenerateToken(
                Arg.Any<User>());

        _jwtProvider
            .DidNotReceive()
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenTokenIsRevoked()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "revoked-token"
        };

        var user = CreateTestUser();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = true
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var command = new RefreshTokenCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ForbiddenException>();

        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        _jwtProvider
            .DidNotReceive()
            .GenerateToken(
                Arg.Any<User>());

        _jwtProvider
            .DidNotReceive()
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenTokenIsExpired()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "expired-token"
        };

        var user = CreateTestUser();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Revoked = false
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var command = new RefreshTokenCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ForbiddenException>();

        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        _jwtProvider
            .DidNotReceive()
            .GenerateToken(
                Arg.Any<User>());

        _jwtProvider
            .DidNotReceive()
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenTokenIsValid()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "valid-token"
        };

        var user = CreateTestUser();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = false
        };

        var expectedNewAccessToken = "new-access-token";
        var expectedNewRefreshToken = "new-refresh-token";

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        _jwtProvider
            .GenerateToken(user)
            .Returns(expectedNewAccessToken);

        _jwtProvider
            .GenerateRefreshToken()
            .Returns(expectedNewRefreshToken);

        var command = new RefreshTokenCommand(dto);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedNewAccessToken);
        result.RefreshToken.Should().Be(expectedNewRefreshToken);

        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        _jwtProvider
            .Received(1)
            .GenerateToken(user);

        _jwtProvider
            .Received(1)
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.Id == storedToken.Id &&
                    rt.Revoked == true),
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.Id != Guid.Empty &&
                    rt.UserId == user.Id &&
                    rt.Token == expectedNewRefreshToken &&
                    rt.ExpiresAt > DateTime.UtcNow.AddDays(29) &&
                    rt.ExpiresAt < DateTime.UtcNow.AddDays(31)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRevokeOldToken_WhenTokenIsValid()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "valid-token"
        };

        var user = CreateTestUser();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = false
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        _jwtProvider
            .GenerateToken(user)
            .Returns("new-access-token");

        _jwtProvider
            .GenerateRefreshToken()
            .Returns("new-refresh-token");

        var command = new RefreshTokenCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        storedToken.Revoked.Should().BeTrue();

        await _refreshTokenRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.Id == storedToken.Id &&
                    rt.Revoked == true &&
                    rt.Token == storedToken.Token &&
                    rt.UserId == storedToken.UserId &&
                    rt.ExpiresAt == storedToken.ExpiresAt),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateNewRefreshTokenWithCorrectData_WhenTokenIsValid()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "valid-token"
        };

        var user = CreateTestUser();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = false
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        _jwtProvider
            .GenerateToken(user)
            .Returns("new-access-token");

        _jwtProvider
            .GenerateRefreshToken()
            .Returns("new-refresh-token");

        var command = new RefreshTokenCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _refreshTokenRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.UserId == user.Id &&
                    rt.Token == "new-refresh-token" &&
                    rt.Id != Guid.Empty &&
                    rt.Revoked == false &&
                    rt.ExpiresAt > DateTime.UtcNow.AddDays(29) &&
                    rt.ExpiresAt < DateTime.UtcNow.AddDays(31)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUseUserFromStoredToken_WhenGeneratingNewToken()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "valid-token"
        };

        var user = CreateTestUser();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = false
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        _jwtProvider
            .GenerateToken(user)
            .Returns("new-access-token");

        _jwtProvider
            .GenerateRefreshToken()
            .Returns("new-refresh-token");

        var command = new RefreshTokenCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _jwtProvider
            .Received(1)
            .GenerateToken(
                Arg.Is<User>(u =>
                    u.Id == user.Id &&
                    u.Username == user.Username &&
                    u.Email == user.Email));
    }

    private static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "john",
            Email = "john@example.com",
            PasswordHash = "hashed-password",
            IsDeleted = false,
            AppRole = AppRole.User
        };
    }
}