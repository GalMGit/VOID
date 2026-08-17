using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Users.Queries.SearchUsers;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Users;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    private readonly SearchUsersQueryHandler _sut;

    public SearchUsersQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new SearchUsersQueryHandler(
            _userRepository,
            _mapper);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSearchResults_WhenUsersFound()
    {
        // Arrange
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

        _userRepository
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>())
            .Returns(users);

        _mapper
            .Map<List<SearchUserDto>>(users)
            .Returns(searchUserDtos);

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(searchUserDtos);

        await _userRepository
            .Received(1)
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<List<SearchUserDto>>(users);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoUsersFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var searchTerm = "xyz";

        _userRepository
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        _mapper
            .Map<List<SearchUserDto>>(Arg.Any<List<User>>())
            .Returns(new List<SearchUserDto>());

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectSearchTerm_WhenSearching()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var searchTerm = "john";
        var differentSearchTerm = "jane";

        _userRepository
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        _mapper
            .Map<List<SearchUserDto>>(Arg.Any<List<User>>())
            .Returns(new List<SearchUserDto>());

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .SearchAsync(differentSearchTerm, userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectUserId_WhenSearching()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var searchTerm = "john";

        _userRepository
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        _mapper
            .Map<List<SearchUserDto>>(Arg.Any<List<User>>())
            .Returns(new List<SearchUserDto>());

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .SearchAsync(searchTerm, differentUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldMapUsersToSearchUserDtos_WhenUsersFound()
    {
        // Arrange
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

        _userRepository
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>())
            .Returns(users);

        _mapper
            .Map<List<SearchUserDto>>(users)
            .Returns(searchUserDtos);

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<List<SearchUserDto>>(
                Arg.Is<List<User>>(u =>
                    u.Count == 1 &&
                    u[0].Username == "alice"));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSearchUserDtoWithCorrectProperties_WhenUsersFound()
    {
        // Arrange
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

        _userRepository
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>())
            .Returns(users);

        _mapper
            .Map<List<SearchUserDto>>(users)
            .Returns(searchUserDtos);

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result[0].Id.Should().Be(bobId);
        result[0].Username.Should().Be("bob");
        result[0].AvatarUrl.Should().Be("bob-avatar.jpg");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenMapperReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var searchTerm = "john";

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Username = "john",
                Email = "john@example.com"
            }
        };

        _userRepository
            .SearchAsync(searchTerm, userId, Arg.Any<CancellationToken>())
            .Returns(users);

        _mapper
            .Map<List<SearchUserDto>>(users)
            .Returns((List<SearchUserDto>?)null!);

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mapper
            .Received(1)
            .Map<List<SearchUserDto>>(users);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseCorrectCancellationToken_WhenSearching()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var searchTerm = "john";
        var ct = new CancellationTokenSource().Token;

        _userRepository
            .SearchAsync(searchTerm, userId, ct)
            .Returns(new List<User>());

        _mapper
            .Map<List<SearchUserDto>>(Arg.Any<List<User>>())
            .Returns(new List<SearchUserDto>());

        var query = new SearchUsersQuery(searchTerm, userId);

        // Act
        await _sut.HandleAsync(query, ct);

        // Assert
        await _userRepository
            .Received(1)
            .SearchAsync(searchTerm, userId, ct);
    }
}