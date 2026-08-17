using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Queries.SearchUsersForGroup;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Users;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.Queries.SearchUsersForGroup;

public sealed class SearchUsersForGroupQueryHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    private readonly SearchUsersForGroupQueryHandler _sut;

    public SearchUsersForGroupQueryHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new SearchUsersForGroupQueryHandler(
            _groupRepository,
            _userRepository,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotGroupMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTerm = "john";

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new SearchUsersForGroupQuery(searchTerm, userId, groupId);

        // Act
        var act = () => _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ForbiddenException>();

        await _groupRepository
            .Received(1)
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .SearchUsersForGroupAsync(
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<List<SearchUserDto>>(
                Arg.Any<List<User>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSearchResults_WhenUserIsGroupMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTerm = "jo";

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Username = "john",
                Email = "john@example.com"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Username = "joanna",
                Email = "joanna@example.com"
            }
        };

        var searchUserDtos = new List<SearchUserDto>
        {
            new()
            {
                Id = users[0].Id,
                Username = "john",
                AvatarUrl = null
            },
            new()
            {
                Id = users[1].Id,
                Username = "joanna",
                AvatarUrl = null
            }
        };

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _userRepository
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(users);

        _mapper
            .Map<List<SearchUserDto>>(users)
            .Returns(searchUserDtos);

        var query = new SearchUsersForGroupQuery(searchTerm, userId, groupId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(searchUserDtos);

        await _groupRepository
            .Received(1)
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _userRepository
            .Received(1)
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<List<SearchUserDto>>(users);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoUsersFound()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTerm = "xyz";

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _userRepository
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        _mapper
            .Map<List<SearchUserDto>>(Arg.Any<List<User>>())
            .Returns(new List<SearchUserDto>());

        var query = new SearchUsersForGroupQuery(searchTerm, userId, groupId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParameters_WhenSearching()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTerm = "test";
        var differentGroupId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _userRepository
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        _mapper
            .Map<List<SearchUserDto>>(Arg.Any<List<User>>())
            .Returns(new List<SearchUserDto>());

        var query = new SearchUsersForGroupQuery(searchTerm, userId, groupId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .SearchUsersForGroupAsync(
                searchTerm,
                differentUserId,
                groupId,
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                differentGroupId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapUsersToSearchUserDtos_WhenUsersFound()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTerm = "a";

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Username = "alice",
                Email = "alice@example.com"
            }
        };

        var searchUserDtos = new List<SearchUserDto>
        {
            new()
            {
                Id = users[0].Id,
                Username = "alice",
                AvatarUrl = "avatar.jpg"
            }
        };

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _userRepository
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(users);

        _mapper
            .Map<List<SearchUserDto>>(users)
            .Returns(searchUserDtos);

        var query = new SearchUsersForGroupQuery(searchTerm, userId, groupId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<List<SearchUserDto>>(
                Arg.Is<List<User>>(u =>
                    u.Count == 1 &&
                    u[0].Username == "alice"));
    }

    [Fact]
    public async Task Handle_ShouldCheckMembership_WhenSearchingUsers()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTerm = "test";

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _userRepository
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        _mapper
            .Map<List<SearchUserDto>>(Arg.Any<List<User>>())
            .Returns(new List<SearchUserDto>());

        var query = new SearchUsersForGroupQuery(searchTerm, userId, groupId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSearchUserDtoWithCorrectProperties_WhenUsersFound()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTerm = "bob";
        var bobId = Guid.NewGuid();

        var users = new List<User>
        {
            new()
            {
                Id = bobId,
                Username = "bob",
                Email = "bob@example.com"
            }
        };

        var searchUserDtos = new List<SearchUserDto>
        {
            new()
            {
                Id = bobId,
                Username = "bob",
                AvatarUrl = "bob-avatar.jpg"
            }
        };

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _userRepository
            .SearchUsersForGroupAsync(
                searchTerm,
                userId,
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(users);

        _mapper
            .Map<List<SearchUserDto>>(users)
            .Returns(searchUserDtos);

        var query = new SearchUsersForGroupQuery(searchTerm, userId, groupId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result[0].Id.Should().Be(bobId);
        result[0].Username.Should().Be("bob");
        result[0].AvatarUrl.Should().Be("bob-avatar.jpg");
    }
}