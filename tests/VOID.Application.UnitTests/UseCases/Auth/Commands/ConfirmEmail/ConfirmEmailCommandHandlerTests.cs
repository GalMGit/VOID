using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Auth.Commands.ConfirmEmail;
using VOID.Domain.Enums.Roles.App;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Auth.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;

    private readonly ConfirmEmailCommandHandler _sut;

    public ConfirmEmailCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _cacheService = Substitute.For<ICacheService>();

        _sut = new ConfirmEmailCommandHandler(
            _userRepository,
            _cacheService);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTempUserDoesNotExist()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "12345"
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns((TempUser?)null);

        var command = new ConfirmEmailCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _cacheService
            .Received(1)
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .EmailExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .UsernameExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>());

        await _cacheService
            .DidNotReceive()
            .RemoveAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenConfirmationCodeIsIncorrect()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "99999"
        };

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Username = "john",
            Name = "john",
            PasswordHash = "hashed-password",
            Role = AppRole.User,
            ConfirmationCode = "12345",
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(tempUser);

        var command = new ConfirmEmailCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ValidationException>();

        await _cacheService
            .Received(1)
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .EmailExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .UsernameExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>());

        await _cacheService
            .DidNotReceive()
            .RemoveAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCodeHasExpired()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "12345"
        };

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Username = "john",
            Name = "john",
            PasswordHash = "hashed-password",
            Role = AppRole.User,
            ConfirmationCode = "12345",
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(tempUser);

        var command = new ConfirmEmailCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ValidationException>();

        await _cacheService
            .Received(1)
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .EmailExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .UsernameExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>());

        await _cacheService
            .DidNotReceive()
            .RemoveAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "12345"
        };

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Username = "john",
            Name = "john",
            PasswordHash = "hashed-password",
            Role = AppRole.User,
            ConfirmationCode = "12345",
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(tempUser);

        _userRepository
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new ConfirmEmailCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _cacheService
            .Received(1)
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .UsernameExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>());

        await _cacheService
            .DidNotReceive()
            .RemoveAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "12345"
        };

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Username = "john",
            Name = "john",
            PasswordHash = "hashed-password",
            Role = AppRole.User,
            ConfirmationCode = "12345",
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(tempUser);

        _userRepository
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository
            .UsernameExistsAsync(
                tempUser.Username,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new ConfirmEmailCommand(dto);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _cacheService
            .Received(1)
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .UsernameExistsAsync(
                tempUser.Username,
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>());

        await _cacheService
            .DidNotReceive()
            .RemoveAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndRemoveCache_WhenAllValidationsPass()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "12345"
        };

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Username = "john",
            Name = "John Doe",
            PasswordHash = "hashed-password",
            Role = AppRole.User,
            ConfirmationCode = "12345",
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(tempUser);

        _userRepository
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository
            .UsernameExistsAsync(
                tempUser.Username,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new ConfirmEmailCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _cacheService
            .Received(1)
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .UsernameExistsAsync(
                tempUser.Username,
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<User>(user =>
                    user.Id == tempUser.Id &&
                    user.CreatedAt == tempUser.CreatedAt &&
                    user.Name == tempUser.Name &&
                    user.Username == tempUser.Username &&
                    user.Email == tempUser.Email &&
                    user.EmailConfirmed == true &&
                    user.AppRole == tempUser.Role &&
                    user.PasswordHash == tempUser.PasswordHash &&
                    user.LastSeen > DateTime.UtcNow.AddSeconds(-5) &&
                    user.LastSeen < DateTime.UtcNow.AddSeconds(5)),
                Arg.Any<CancellationToken>());

        await _cacheService
            .Received(1)
            .RemoveAsync(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetEmailConfirmedToTrue_WhenUserCreated()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "12345"
        };

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Username = "john",
            Name = "John Doe",
            PasswordHash = "hashed-password",
            Role = AppRole.User,
            ConfirmationCode = "12345",
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(tempUser);

        _userRepository
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository
            .UsernameExistsAsync(
                tempUser.Username,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new ConfirmEmailCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<User>(user =>
                    user.EmailConfirmed == true),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRemoveCache_AfterSuccessfulUserCreation()
    {
        // Arrange
        var dto = new ConfirmEmailDto
        {
            Email = "john@example.com",
            Code = "12345"
        };

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Username = "john",
            Name = "John Doe",
            PasswordHash = "hashed-password",
            Role = AppRole.User,
            ConfirmationCode = "12345",
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _cacheService
            .GetAsync<TempUser>(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(tempUser);

        _userRepository
            .EmailExistsAsync(
                tempUser.Email,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository
            .UsernameExistsAsync(
                tempUser.Username,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new ConfirmEmailCommand(dto);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _cacheService
            .Received(1)
            .RemoveAsync(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());
    }
}