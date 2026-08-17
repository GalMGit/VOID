using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Auth.Commands.Logout;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Logout;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Auth.Commands.Logout;

public sealed class LogoutUserCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    private readonly LogoutUserCommandHandler _sut;

    public LogoutUserCommandHandlerTests()
    {
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

        _sut = new LogoutUserCommandHandler(
            _refreshTokenRepository);
    }

    [Fact]
    public async Task Handle_ShouldRevokeToken_WhenTokenExists()
    {
        // Arrange
        var dto = new LogoutDto
        {
            RefreshToken = "valid-refresh-token"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = false
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var command = new LogoutUserCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.Id == storedToken.Id &&
                    rt.Token == storedToken.Token &&
                    rt.Revoked == true),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotUpdate_WhenTokenDoesNotExist()
    {
        // Arrange
        var dto = new LogoutDto
        {
            RefreshToken = "non-existent-token"
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var command = new LogoutUserCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetRevokedToTrue_WhenTokenWasNotRevoked()
    {
        // Arrange
        var dto = new LogoutDto
        {
            RefreshToken = "active-token"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = false
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var command = new LogoutUserCommand(dto);

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
                    rt.Revoked == true &&
                    rt.Id == storedToken.Id),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRevokeToken_WhenTokenWasAlreadyRevoked()
    {
        // Arrange
        var dto = new LogoutDto
        {
            RefreshToken = "already-revoked-token"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = dto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Revoked = true
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var command = new LogoutUserCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.Id == storedToken.Id &&
                    rt.Revoked == true),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenTokenIsEmpty()
    {
        // Arrange
        var dto = new LogoutDto
        {
            RefreshToken = string.Empty
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var command = new LogoutUserCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .NotThrowAsync();

        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenTokenIsNull()
    {
        // Arrange
        var dto = new LogoutDto
        {
            RefreshToken = null!
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var command = new LogoutUserCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .NotThrowAsync();

        await _refreshTokenRepository
            .Received(1)
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>());

        await _refreshTokenRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPreserveTokenProperties_WhenRevoking()
    {
        // Arrange
        var dto = new LogoutDto
        {
            RefreshToken = "token-to-revoke"
        };

        var userId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(15);

        var storedToken = new RefreshToken
        {
            Id = tokenId,
            UserId = userId,
            Token = dto.RefreshToken,
            ExpiresAt = expiresAt,
            Revoked = false
        };

        _refreshTokenRepository
            .GetByTokenAsync(
                dto.RefreshToken,
                Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var command = new LogoutUserCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _refreshTokenRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.Id == tokenId &&
                    rt.UserId == userId &&
                    rt.Token == dto.RefreshToken &&
                    rt.ExpiresAt == expiresAt &&
                    rt.Revoked == true),
                Arg.Any<CancellationToken>());
    }
}