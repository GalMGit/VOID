using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Auth.Commands.Register;
using VOID.Application.UseCases.Auth.Events;
using VOID.Domain.Enums.Roles.App;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Auth.Commands.Register;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMessageBus _bus;

    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _cacheService = Substitute.For<ICacheService>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new RegisterUserCommandHandler(
            _userRepository,
            _cacheService,
            _passwordHasher,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenUsernameAlreadyExists()
    {
        var dto = new RegisterUserDto
        {
            Email = "john@example.com",
            Username = "john",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepository.UsernameExistsAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new RegisterUserCommand(dto);

        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _userRepository
            .DidNotReceive()
            .EmailExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _passwordHasher
            .DidNotReceive()
            .GenerateHash(
                Arg.Any<string>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<UserStartRegistrationEvent>());
    }
    
    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        var dto = new RegisterUserDto
        {
            Email = "john@example.com",
            Username = "john",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepository
            .UsernameExistsAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository
            .EmailExistsAsync(
                dto.Email,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new RegisterUserCommand(dto);

        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _userRepository
            .Received(1)
            .UsernameExistsAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .EmailExistsAsync(
                dto.Email,
                Arg.Any<CancellationToken>());

        await _cacheService
            .DidNotReceive()
            .ExistsAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _passwordHasher
            .DidNotReceive()
            .GenerateHash(
                Arg.Any<string>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<UserStartRegistrationEvent>());
    }
    
    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenConfirmationCodeAlreadySent()
    {
        var dto = new RegisterUserDto
        {
            Email = "john@example.com",
            Username = "john",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepository
            .UsernameExistsAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository
            .EmailExistsAsync(
                dto.Email,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _cacheService
            .ExistsAsync(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new RegisterUserCommand(dto);

        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _cacheService
            .Received(1)
            .ExistsAsync(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>());

        _passwordHasher
            .DidNotReceive()
            .GenerateHash(
                Arg.Any<string>());

        await _cacheService
            .DidNotReceive()
            .SetAsync(
                Arg.Any<string>(),
                Arg.Any<TempUser>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<UserStartRegistrationEvent>());
    }
    
    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenUsernameEmailAndConfirmationCodeDoNotExist()
    {
        var dto = new RegisterUserDto
        {
            Email = "john@gmail.com",
            Username = "john",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepository
            .UsernameExistsAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository
            .EmailExistsAsync(
                dto.Email,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _cacheService
            .ExistsAsync(
                $"temp_user:{dto.Email}",
                Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher
            .GenerateHash(dto.Password)
            .Returns("hashed-password");

        var command = new RegisterUserCommand(dto);
        
        var result = await _sut.Handle(
            command,
            CancellationToken.None);
        
        result.Should().NotBeNull();
        result.Email.Should().Be(dto.Email);

        _passwordHasher
            .Received(1)
            .GenerateHash(dto.Password);

        await _cacheService
            .Received(1)
            .SetAsync(
                $"temp_user:{dto.Email}",
                Arg.Is<TempUser>(user =>
                    user.Email == dto.Email &&
                    user.Username == dto.Username &&
                    user.Name == dto.Username &&
                    user.PasswordHash == "hashed-password" &&
                    user.Role == AppRole.User &&
                    user.ConfirmationCode != null &&
                    user.ConfirmationCode.Length == 5 &&
                    user.CodeExpiresAt > DateTime.UtcNow),
                TimeSpan.FromMinutes(10),
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStartRegistrationEvent>(e =>
                    e.Email == dto.Email &&
                    e.Username == dto.Username &&
                    !string.IsNullOrEmpty(e.ConfirmationCode)));
    }
}