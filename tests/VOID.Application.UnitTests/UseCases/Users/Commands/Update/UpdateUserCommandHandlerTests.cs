using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Users.Commands.Update;
using VOID.Application.UseCases.Users.Events.Profile;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Users.Accounts;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Users.Commands.Update;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IMessageBus _bus;

    private readonly UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new UpdateUserCommandHandler(
            _userRepository,
            _mapper,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = "New Name",
            AboutMe = "New About"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _userRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenDataIsSame()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = "John",
            AboutMe = "About John"
        };

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = "About John"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        await _userRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateName_WhenNameChanged()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = "New Name",
            AboutMe = "About John"
        };

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = "About John"
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "john",
            Name = "New Name",
            AboutMe = "About John"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Name = "New Name"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository
            .UpdateAsync(user, Arg.Any<CancellationToken>())
            .Returns(updatedUser);

        _mapper
            .Map<UserAuthDto>(updatedUser)
            .Returns(userDto);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(userDto);

        await _userRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<User>(u =>
                    u.Name == "New Name" &&
                    u.AboutMe == "About John"),
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserUpdatedEvent>(e =>
                    e.UserId == userId &&
                    e.Name == "New Name"));
    }

    [Fact]
    public async Task Handle_ShouldUpdateAboutMe_WhenAboutMeChanged()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = "John",
            AboutMe = "New About"
        };

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = "Old About"
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = "New About"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Name = "John"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository
            .UpdateAsync(user, Arg.Any<CancellationToken>())
            .Returns(updatedUser);

        _mapper
            .Map<UserAuthDto>(updatedUser)
            .Returns(userDto);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        await _userRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<User>(u =>
                    u.Name == "John" &&
                    u.AboutMe == "New About"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetNameToUsername_WhenNameIsNullOrEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = null,
            AboutMe = "About John"
        };

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "Old Name",
            AboutMe = "About John"
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "john",
            Name = "john",
            AboutMe = "About John"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Name = "john"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository
            .UpdateAsync(user, Arg.Any<CancellationToken>())
            .Returns(updatedUser);

        _mapper
            .Map<UserAuthDto>(updatedUser)
            .Returns(userDto);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<User>(u =>
                    u.Name == "john"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetAboutMeToNull_WhenAboutMeIsNullOrEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = "John",
            AboutMe = null
        };

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = "Old About"
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = null
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Name = "John"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository
            .UpdateAsync(user, Arg.Any<CancellationToken>())
            .Returns(updatedUser);

        _mapper
            .Map<UserAuthDto>(updatedUser)
            .Returns(userDto);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<User>(u =>
                    u.AboutMe == null),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenUserUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = "Updated Name",
            AboutMe = "Updated About"
        };

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = "Old About"
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "john",
            Name = "Updated Name",
            AboutMe = "Updated About"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Name = "Updated Name"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository
            .UpdateAsync(user, Arg.Any<CancellationToken>())
            .Returns(updatedUser);

        _mapper
            .Map<UserAuthDto>(updatedUser)
            .Returns(userDto);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserUpdatedEvent>(e =>
                    e.UserId == userId &&
                    e.Name == "Updated Name"));
    }

    [Fact]
    public async Task Handle_ShouldMapUpdatedUser_WhenUserUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto
        {
            Name = "New Name",
            AboutMe = "New About"
        };

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John",
            AboutMe = "Old About"
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "john",
            Name = "New Name",
            AboutMe = "New About"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Name = "New Name"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository
            .UpdateAsync(user, Arg.Any<CancellationToken>())
            .Returns(updatedUser);

        _mapper
            .Map<UserAuthDto>(updatedUser)
            .Returns(userDto);

        var command = new UpdateUserCommand(dto, userId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(userDto);

        _mapper
            .Received(1)
            .Map<UserAuthDto>(updatedUser);
    }
}