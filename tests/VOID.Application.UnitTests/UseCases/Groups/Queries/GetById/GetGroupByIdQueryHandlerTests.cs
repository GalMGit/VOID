using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Queries.GetById;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.Queries.GetById;

public sealed class GetGroupByIdQueryHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    private readonly GetGroupByIdQueryHandler _sut;

    public GetGroupByIdQueryHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _messageRepository = Substitute.For<IMessageRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetGroupByIdQueryHandler(
            _groupRepository,
            _messageRepository,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns((GroupChat?)null);

        var query = new GetGroupByIdQuery(userId, groupId);

        // Act
        var act = () => _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _groupRepository
            .Received(1)
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .GetTotalCountByGroupAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<FullGroupDto>(
                Arg.Any<GroupChat>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotMemberOfGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = otherUserId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = otherUserId,
            GroupRole = GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        var query = new GetGroupByIdQuery(userId, groupId);

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
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .GetTotalCountByGroupAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<FullGroupDto>(
                Arg.Any<GroupChat>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFullGroupDto_WhenUserIsMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var messageCount = 15;

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        var fullGroupDto = new FullGroupDto
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId
        };

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        _messageRepository
            .GetTotalCountByGroupAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(messageCount);

        _mapper
            .Map<FullGroupDto>(group)
            .Returns(fullGroupDto);

        var query = new GetGroupByIdQuery(userId, groupId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(fullGroupDto);
        result.Id.Should().Be(groupId);
        result.MessageCount.Should().Be(messageCount);

        await _groupRepository
            .Received(1)
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _messageRepository
            .Received(1)
            .GetTotalCountByGroupAsync(
                groupId,
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<FullGroupDto>(group);
    }

    [Fact]
    public async Task Handle_ShouldSetMessageCount_WhenReturningGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedMessageCount = 25;

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        var fullGroupDto = new FullGroupDto
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId
        };

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        _messageRepository
            .GetTotalCountByGroupAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(expectedMessageCount);

        _mapper
            .Map<FullGroupDto>(group)
            .Returns(fullGroupDto);

        var query = new GetGroupByIdQuery(userId, groupId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.MessageCount.Should().Be(expectedMessageCount);
    }

    [Fact]
    public async Task Handle_ShouldGetMessageCount_WhenUserIsMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        var fullGroupDto = new FullGroupDto
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId
        };

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        _messageRepository
            .GetTotalCountByGroupAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(5);

        _mapper
            .Map<FullGroupDto>(group)
            .Returns(fullGroupDto);

        var query = new GetGroupByIdQuery(userId, groupId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .GetTotalCountByGroupAsync(
                groupId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldWorkWithZeroMessageCount_WhenGroupHasNoMessages()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        var fullGroupDto = new FullGroupDto
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId
        };

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        _messageRepository
            .GetTotalCountByGroupAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _mapper
            .Map<FullGroupDto>(group)
            .Returns(fullGroupDto);

        var query = new GetGroupByIdQuery(userId, groupId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldMapGroup_WhenUserIsMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        var fullGroupDto = new FullGroupDto
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId
        };

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        _messageRepository
            .GetTotalCountByGroupAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _mapper
            .Map<FullGroupDto>(group)
            .Returns(fullGroupDto);

        var query = new GetGroupByIdQuery(userId, groupId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<FullGroupDto>(
                Arg.Is<GroupChat>(g => g.Id == groupId));
    }
}