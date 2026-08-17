using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.UseCases.Groups.Queries.GetGroupsByUser;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;
using VOID.Shared.Contracts.DTOs.Paginations;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.Queries.GetGroupsByUser;

public sealed class GetGroupsByUserQueryHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;

    private readonly GetGroupsByUserQueryHandler _sut;

    public GetGroupsByUserQueryHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetGroupsByUserQueryHandler(
            _groupRepository,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult_WhenGroupsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var totalCount = 2;
        var groupId1 = Guid.NewGuid();
        var groupId2 = Guid.NewGuid();

        var groups = new List<GroupChat>
        {
            new()
            {
                Id = groupId1,
                ChatName = "Group 1",
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = groupId2,
                ChatName = "Group 2",
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            }
        };

        var groupDtos = new List<GroupDto>
        {
            new() { Id = groupId1, ChatName = "Group 1", OwnerId = userId },
            new() { Id = groupId2, ChatName = "Group 2", OwnerId = userId }
        };

        _groupRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(totalCount);

        _groupRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(groups);

        _mapper
            .Map<List<GroupDto>>(groups)
            .Returns(groupDtos);

        var query = new GetGroupsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().BeEquivalentTo(groupDtos);

        await _groupRepository
            .Received(1)
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<List<GroupDto>>(groups);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoGroupsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        _groupRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(new List<GroupChat>());

        _mapper
            .Map<List<GroupDto>>(Arg.Any<List<GroupChat>>())
            .Returns(new List<GroupDto>());

        var query = new GetGroupsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldUsePaginationParameters_WhenGettingGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(3, 20);

        _groupRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(new List<GroupChat>());

        _mapper
            .Map<List<GroupDto>>(Arg.Any<List<GroupChat>>())
            .Returns(new List<GroupDto>());

        var query = new GetGroupsByUserQuery(userId, pagination);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .GetAllByUserAsync(
                userId,
                Arg.Is<PaginationRequest>(p =>
                    p.PageNumber == 3 &&
                    p.PageSize == 20),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserId_WhenGettingGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);

        _groupRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(new List<GroupChat>());

        _mapper
            .Map<List<GroupDto>>(Arg.Any<List<GroupChat>>())
            .Returns(new List<GroupDto>());

        var query = new GetGroupsByUserQuery(userId, pagination);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .GetTotalCountByUserAsync(
                differentUserId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapGroupsToDtos_WhenGroupsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var groupId1 = Guid.NewGuid();
        var groupId2 = Guid.NewGuid();

        var groups = new List<GroupChat>
        {
            new()
            {
                Id = groupId1,
                ChatName = "Group 1",
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = groupId2,
                ChatName = "Group 2",
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            }
        };

        var groupDtos = new List<GroupDto>
        {
            new() { Id = groupId1, ChatName = "Group 1", OwnerId = userId },
            new() { Id = groupId2, ChatName = "Group 2", OwnerId = userId }
        };

        _groupRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(2);

        _groupRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(groups);

        _mapper
            .Map<List<GroupDto>>(groups)
            .Returns(groupDtos);

        var query = new GetGroupsByUserQuery(userId, pagination);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<List<GroupDto>>(
                Arg.Is<List<GroupChat>>(g =>
                    g.Count == 2 &&
                    g.Any(x => x.Id == groupId1) &&
                    g.Any(x => x.Id == groupId2)));
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectPaginationInfo_WhenGroupsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(2, 5);
        var totalCount = 10;
        var groupId = Guid.NewGuid();

        var groups = new List<GroupChat>
        {
            new()
            {
                Id = groupId,
                ChatName = "Group 1",
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            }
        };

        var groupDtos = new List<GroupDto>
        {
            new() { Id = groupId, ChatName = "Group 1", OwnerId = userId }
        };

        _groupRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(totalCount);

        _groupRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(groups);

        _mapper
            .Map<List<GroupDto>>(groups)
            .Returns(groupDtos);

        var query = new GetGroupsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnGroupsInCorrectOrder_WhenMultipleGroupsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pagination = new PaginationRequest(1, 10);
        var groupId1 = Guid.NewGuid();
        var groupId2 = Guid.NewGuid();
        var groupId3 = Guid.NewGuid();

        var groups = new List<GroupChat>
        {
            new() { Id = groupId1, ChatName = "Group 1", OwnerId = userId },
            new() { Id = groupId2, ChatName = "Group 2", OwnerId = userId },
            new() { Id = groupId3, ChatName = "Group 3", OwnerId = userId }
        };

        var groupDtos = new List<GroupDto>
        {
            new() { Id = groupId1, ChatName = "Group 1", OwnerId = userId },
            new() { Id = groupId2, ChatName = "Group 2", OwnerId = userId },
            new() { Id = groupId3, ChatName = "Group 3", OwnerId = userId }
        };

        _groupRepository
            .GetTotalCountByUserAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(3);

        _groupRepository
            .GetAllByUserAsync(
                userId,
                pagination,
                Arg.Any<CancellationToken>())
            .Returns(groups);

        _mapper
            .Map<List<GroupDto>>(groups)
            .Returns(groupDtos);

        var query = new GetGroupsByUserQuery(userId, pagination);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Id.Should().Be(groupId1);
        result.Items[1].Id.Should().Be(groupId2);
        result.Items[2].Id.Should().Be(groupId3);
    }
}