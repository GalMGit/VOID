using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Auth.Commands.Login;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Login;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Auth.Commands.Login;

public sealed class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    private readonly LoginUserCommandHandler _sut;

    public LoginUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtProvider = Substitute.For<IJwtProvider>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

        _sut = new LoginUserCommandHandler(
            _userRepository,
            _passwordHasher,
            _jwtProvider,
            _refreshTokenRepository);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var dto = new LoginUserDto
        {
            Username = "nonexistent",
            Password = "Password123!"
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new LoginUserCommand(dto);
        
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);
        
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        _passwordHasher
            .DidNotReceive()
            .VerifyHash(
                Arg.Any<string>(),
                Arg.Any<string>());

        _jwtProvider
            .DidNotReceive()
            .GenerateToken(
                Arg.Any<User>());

        _jwtProvider
            .DidNotReceive()
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserIsDeleted()
    {
        var dto = new LoginUserDto
        {
            Username = "deleteduser",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = "deleted@example.com",
            PasswordHash = "hashed-password",
            IsDeleted = true
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new LoginUserCommand(dto);
        
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);
        
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        _passwordHasher
            .DidNotReceive()
            .VerifyHash(
                Arg.Any<string>(),
                Arg.Any<string>());

        _jwtProvider
            .DidNotReceive()
            .GenerateToken(
                Arg.Any<User>());

        _jwtProvider
            .DidNotReceive()
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenPasswordIsIncorrect()
    {
        var dto = new LoginUserDto
        {
            Username = "john",
            Password = "WrongPassword123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = "john@example.com",
            PasswordHash = "correct-hashed-password",
            IsDeleted = false
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .VerifyHash(
                dto.Password,
                user.PasswordHash)
            .Returns(false);

        var command = new LoginUserCommand(dto);
        
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);
        
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        _passwordHasher
            .Received(1)
            .VerifyHash(
                dto.Password,
                user.PasswordHash);

        _jwtProvider
            .DidNotReceive()
            .GenerateToken(
                Arg.Any<User>());

        _jwtProvider
            .DidNotReceive()
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<RefreshToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnLoginDto_WhenCredentialsAreValid()
    {
        var dto = new LoginUserDto
        {
            Username = "john",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = "john@example.com",
            PasswordHash = "hashed-password",
            IsDeleted = false
        };

        var expectedToken = "access-token";
        var expectedRefreshToken = "refresh-token";

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .VerifyHash(
                dto.Password,
                user.PasswordHash)
            .Returns(true);

        _jwtProvider
            .GenerateToken(user)
            .Returns(expectedToken);

        _jwtProvider
            .GenerateRefreshToken()
            .Returns(expectedRefreshToken);

        var command = new LoginUserCommand(dto);
        
        var result = await _sut.Handle(
            command,
            CancellationToken.None);
        
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.RefreshToken.Should().Be(expectedRefreshToken);

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        _passwordHasher
            .Received(1)
            .VerifyHash(
                dto.Password,
                user.PasswordHash);

        _jwtProvider
            .Received(1)
            .GenerateToken(user);

        _jwtProvider
            .Received(1)
            .GenerateRefreshToken();

        await _refreshTokenRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.Id != Guid.Empty &&
                    rt.UserId == user.Id &&
                    rt.Token == expectedRefreshToken &&
                    rt.ExpiresAt > DateTime.UtcNow.AddDays(29) &&
                    rt.ExpiresAt < DateTime.UtcNow.AddDays(31)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateRefreshTokenWithCorrectExpiration_WhenLoginSuccessful()
    {
        var dto = new LoginUserDto
        {
            Username = "john",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = "john@example.com",
            PasswordHash = "hashed-password",
            IsDeleted = false
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .VerifyHash(
                dto.Password,
                user.PasswordHash)
            .Returns(true);

        _jwtProvider
            .GenerateToken(user)
            .Returns("access-token");

        _jwtProvider
            .GenerateRefreshToken()
            .Returns("refresh-token");

        var command = new LoginUserCommand(dto);
        
        await _sut.Handle(
            command,
            CancellationToken.None);
        
        var before = DateTime.UtcNow.AddDays(30).AddSeconds(-5);
        var after = DateTime.UtcNow.AddDays(30).AddSeconds(5);

        await _refreshTokenRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<RefreshToken>(rt =>
                    rt.ExpiresAt > before &&
                    rt.ExpiresAt < after),
                Arg.Any<CancellationToken>());
    }
}