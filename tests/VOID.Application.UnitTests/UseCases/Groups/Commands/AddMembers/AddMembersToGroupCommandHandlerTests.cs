using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Commands.AddMembers;
using VOID.Application.UseCases.Groups.Events.MembersAdded;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.AddMembers;

public sealed class AddMembersToGroupCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMessageBus _bus;
    private readonly IMapper _mapper;

    private readonly AddMembersToGroupCommandHandler _sut;

    public AddMembersToGroupCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _chatRepository = Substitute.For<IChatRepository>();
        _bus = Substitute.For<IMessageBus>();
        _mapper = Substitute.For<IMapper>();

        _sut = new AddMembersToGroupCommandHandler(
            _groupRepository,
            _chatRepository,
            _bus,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new AddGroupMembersDto
        {
            Members = new List<Guid> { Guid.NewGuid() }
        };

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new AddMembersToGroupCommand(dto, groupId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _groupRepository
            .Received(1)
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .IsMemberAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .GetUsersWithChatsAsync(
                Arg.Any<Guid>(),
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotGroupMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new AddGroupMembersDto
        {
            Members = new List<Guid> { Guid.NewGuid() }
        };

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new AddMembersToGroupCommand(dto, groupId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ForbiddenException>();

        await _groupRepository
            .Received(1)
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .GetUsersWithChatsAsync(
                Arg.Any<Guid>(),
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenMemberAlreadyExistsInGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingMemberId = Guid.NewGuid();

        var dto = new AddGroupMembersDto
        {
            Members = [existingMemberId]
        };

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .GetExistingMemberIdsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid> { existingMemberId });
        
        _chatRepository
            .GetUsersWithChatsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new AddMembersToGroupCommand(dto, groupId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _groupRepository
            .Received(1)
            .GetExistingMemberIdsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>());
        
        await _chatRepository
            .Received(1)
            .GetUsersWithChatsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>());
        
        await _groupRepository
            .DidNotReceive()
            .AddMembersRangeAsync(
                Arg.Any<List<GroupMember>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenNoPersonalChatWithMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var dto = new AddGroupMembersDto
        {
            Members = new List<Guid> { memberId }
        };

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .GetExistingMemberIdsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());

        _chatRepository
            .GetUsersWithChatsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new AddMembersToGroupCommand(dto, groupId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _chatRepository
            .Received(1)
            .GetUsersWithChatsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAddMembers_WhenAllValidationsPass()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();

        var dto = new AddGroupMembersDto
        {
            Members = [memberId1, memberId2]
        };

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var addedMembersWithDetails = new List<GroupMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                MemberId = memberId1,
                GroupRole = GroupRole.Member,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                MemberId = memberId2,
                GroupRole = GroupRole.Member,
                CreatedAt = DateTime.UtcNow
            }
        };

        var memberDtos = new List<GroupMemberDto>
        {
            new() { MemberId = memberId1 },
            new() { MemberId = memberId2 }
        };

        var groupDto = new GroupDto
        {
            Id = groupId,
            ChatName = "Test Group"
        };

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .GetExistingMemberIdsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        _chatRepository
            .GetUsersWithChatsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns( [ memberId1, memberId2 ]);

        _groupRepository
            .GetMembersWithDetailsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(addedMembersWithDetails);

        _mapper
            .Map<List<GroupMemberDto>>(addedMembersWithDetails)
            .Returns(memberDtos);

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        _mapper
            .Map<GroupDto>(group)
            .Returns(groupDto);

        var command = new AddMembersToGroupCommand(dto, groupId, userId);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(memberDtos);

        await _groupRepository
            .Received(1)
            .AddMembersRangeAsync(
                Arg.Is<List<GroupMember>>(members =>
                    members.Count == 2 &&
                    members.All(m => m.GroupId == groupId) &&
                    members.All(m => m.GroupRole == GroupRole.Member) &&
                    members.All(m => m.Id != Guid.Empty) &&
                    members.Any(m => m.MemberId == memberId1) &&
                    members.Any(m => m.MemberId == memberId2)),
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<MembersAddedEvent>(e =>
                    e.Group == groupDto &&
                    e.MembersIds.Count == 2 &&
                    e.MembersIds.Contains(memberId1) &&
                    e.MembersIds.Contains(memberId2) &&
                    e.SenderId == userId));
    }

    [Fact]
    public async Task Handle_ShouldNotAddMembers_WhenMembersListIsEmpty()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var dto = new AddGroupMembersDto
        {
            Members = new List<Guid>()
        };

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .GetExistingMemberIdsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());

        _chatRepository
            .GetUsersWithChatsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        _groupRepository
            .GetMembersWithDetailsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<GroupMember>());

        _mapper
            .Map<List<GroupMemberDto>>(Arg.Any<List<GroupMember>>())
            .Returns(new List<GroupMemberDto>());

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        var groupDto = new GroupDto
        {
            Id = groupId,
            ChatName = "Test Group"
        };

        _mapper
            .Map<GroupDto>(group)
            .Returns(groupDto);

        var command = new AddMembersToGroupCommand(dto, groupId, userId);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        await _groupRepository
            .DidNotReceive()
            .AddMembersRangeAsync(
                Arg.Any<List<GroupMember>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenMembersAdded()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var dto = new AddGroupMembersDto
        {
            Members = new List<Guid> { memberId }
        };

        var group = new GroupChat
        {
            Id = groupId,
            ChatName = "Test Group",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var addedMembersWithDetails = new List<GroupMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                MemberId = memberId,
                GroupRole = GroupRole.Member,
                CreatedAt = DateTime.UtcNow
            }
        };

        var memberDtos = new List<GroupMemberDto>
        {
            new() { MemberId = memberId }
        };

        var groupDto = new GroupDto
        {
            Id = groupId,
            ChatName = "Test Group"
        };

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .GetExistingMemberIdsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        _chatRepository
            .GetUsersWithChatsAsync(
                userId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([memberId]);

        _groupRepository
            .GetMembersWithDetailsAsync(
                groupId,
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(addedMembersWithDetails);

        _mapper
            .Map<List<GroupMemberDto>>(addedMembersWithDetails)
            .Returns(memberDtos);

        _groupRepository
            .GetByIdAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(group);

        _mapper
            .Map<GroupDto>(group)
            .Returns(groupDto);

        var command = new AddMembersToGroupCommand(dto, groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<MembersAddedEvent>(e =>
                    e.Group.Id == groupId &&
                    e.SenderId == userId &&
                    e.MembersIds.Contains(memberId)));
    }
}