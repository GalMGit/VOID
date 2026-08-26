using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Users.Queries.GetUserInfo;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Users.Accounts;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Users.Queries.GetUserInfo;

public sealed class GetUserInfoQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    private readonly GetUserInfoQueryHandler _sut;

    public GetUserInfoQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetUserInfoQueryHandler(
            _userRepository,
            _mapper);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var query = new GetUserInfoQuery(userId);

        // Act
        var act = () => _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        await _userRepository
            .Received(1)
            .GetByIdAsync(userId, Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<UserAuthDto>(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUserAuthDto_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "john",
            Email = "john@example.com",
            Name = "John Doe",
            AboutMe = "About John"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _mapper
            .Map<UserAuthDto>(user)
            .Returns(userDto);

        var query = new GetUserInfoQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(userDto);
        result.Id.Should().Be(userId);

        await _userRepository
            .Received(1)
            .GetByIdAsync(userId, Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<UserAuthDto>(user);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectUserId_WhenGettingUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _mapper
            .Map<UserAuthDto>(user)
            .Returns(userDto);

        var query = new GetUserInfoQuery(userId);

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .GetByIdAsync(userId, Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .GetByIdAsync(differentUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldMapUserToUserAuthDto_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "john",
            Email = "john@example.com",
            Name = "John Doe"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _mapper
            .Map<UserAuthDto>(user)
            .Returns(userDto);

        var query = new GetUserInfoQuery(userId);

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<UserAuthDto>(
                Arg.Is<User>(u => u.Id == userId));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUserAuthDtoWithCorrectProperties_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "john",
            Email = "john@example.com",
            Name = "John Doe",
            AboutMe = "About John"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _mapper
            .Map<UserAuthDto>(user)
            .Returns(userDto);

        var query = new GetUserInfoQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UserAuthDto>();
        result.Id.Should().Be(userId);
        result.Username.Should().Be("john");
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenMapperReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        _userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _mapper
            .Map<UserAuthDto>(user)
            .Returns((UserAuthDto?)null!);

        var query = new GetUserInfoQuery(userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mapper
            .Received(1)
            .Map<UserAuthDto>(user);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseCorrectCancellationToken_WhenGettingUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ct = new CancellationTokenSource().Token;

        var user = new User
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        var userDto = new UserAuthDto
        {
            Id = userId,
            Username = "john",
            Name = "John Doe"
        };

        _userRepository
            .GetByIdAsync(userId, ct)
            .Returns(user);

        _mapper
            .Map<UserAuthDto>(user)
            .Returns(userDto);

        var query = new GetUserInfoQuery(userId);

        // Act
        await _sut.HandleAsync(query, ct);

        // Assert
        await _userRepository
            .Received(1)
            .GetByIdAsync(userId, ct);
    }
}