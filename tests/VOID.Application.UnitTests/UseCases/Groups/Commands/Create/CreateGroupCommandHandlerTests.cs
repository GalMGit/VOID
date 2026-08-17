using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Commands.Create;
using VOID.Application.UseCases.Groups.Events.Created;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.Create;

public sealed class CreateGroupCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly IMessageBus _bus;

    private readonly CreateGroupCommandHandler _sut;

    public CreateGroupCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _mapper = Substitute.For<IMapper>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new CreateGroupCommandHandler(
            _groupRepository,
            _mapper,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenUserHasThreeOrMoreGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateGroupDto
        {
            GroupName = "Test Group"
        };

        _groupRepository
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(3);

        var command = new CreateGroupCommand(dto, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ValidationException>();

        await _groupRepository
            .Received(1)
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<GroupChat>(),
                Arg.Any<CancellationToken>());

        _mapper
            .DidNotReceive()
            .Map<GroupDto>(
                Arg.Any<GroupChat>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<GroupCreatedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldCreateGroup_WhenUserHasLessThanThreeGroups()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateGroupDto
        {
            GroupName = "Test Group"
        };

        var createdGroup = new GroupChat
        {
            Id = Guid.NewGuid(),
            ChatName = dto.GroupName,
            Description = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        createdGroup.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = createdGroup.CreatedAt,
            GroupId = createdGroup.Id
        });

        var groupDto = new GroupDto
        {
            Id = createdGroup.Id,
            ChatName = dto.GroupName,
            OwnerId = userId,
            CreatedAt = createdGroup.CreatedAt
        };

        _groupRepository
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .CreateAsync(
                Arg.Any<GroupChat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdGroup);

        _mapper
            .Map<GroupDto>(createdGroup)
            .Returns(groupDto);

        var command = new CreateGroupCommand(dto, userId);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(groupDto);
        result.Id.Should().Be(createdGroup.Id);
        result.ChatName.Should().Be(dto.GroupName);
        result.OwnerId.Should().Be(userId);

        await _groupRepository
            .Received(1)
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<GroupChat>(g =>
                    g.Id != Guid.Empty &&
                    g.ChatName == dto.GroupName &&
                    g.OwnerId == userId &&
                    g.GroupMembers.Count == 1 &&
                    g.GroupMembers.Any(m => 
                        m.MemberId == userId &&
                        m.GroupRole == GroupRole.Owner &&
                        m.GroupId == g.Id &&
                        m.CreatedAt == g.CreatedAt)),
                Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<GroupDto>(createdGroup);

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupCreatedEvent>(e =>
                    e.Group == groupDto &&
                    e.UserId == userId));
    }

    [Fact]
    public async Task Handle_ShouldCreateGroupWithOwner_WhenCreatingGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateGroupDto
        {
            GroupName = "Test Group"
        };

        var createdGroup = new GroupChat
        {
            Id = Guid.NewGuid(),
            ChatName = dto.GroupName,
            Description = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        createdGroup.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = createdGroup.CreatedAt,
            GroupId = createdGroup.Id
        });

        var groupDto = new GroupDto
        {
            Id = createdGroup.Id,
            ChatName = dto.GroupName,
            OwnerId = userId,
            CreatedAt = createdGroup.CreatedAt
        };

        _groupRepository
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(1);

        _groupRepository
            .CreateAsync(
                Arg.Any<GroupChat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdGroup);

        _mapper
            .Map<GroupDto>(createdGroup)
            .Returns(groupDto);

        var command = new CreateGroupCommand(dto, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<GroupChat>(g =>
                    g.GroupMembers.Count == 1 &&
                    g.GroupMembers.Any(m => 
                        m.MemberId == userId &&
                        m.GroupRole == GroupRole.Owner &&
                        m.GroupId == g.Id &&
                        m.CreatedAt == g.CreatedAt)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenGroupCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateGroupDto
        {
            GroupName = "Test Group"
        };

        var createdGroup = new GroupChat
        {
            Id = Guid.NewGuid(),
            ChatName = dto.GroupName,
            Description = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        createdGroup.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = createdGroup.CreatedAt,
            GroupId = createdGroup.Id
        });

        var groupDto = new GroupDto
        {
            Id = createdGroup.Id,
            ChatName = dto.GroupName,
            OwnerId = userId,
            CreatedAt = createdGroup.CreatedAt
        };

        _groupRepository
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(2);

        _groupRepository
            .CreateAsync(
                Arg.Any<GroupChat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdGroup);

        _mapper
            .Map<GroupDto>(createdGroup)
            .Returns(groupDto);

        var command = new CreateGroupCommand(dto, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupCreatedEvent>(e =>
                    e.Group.Id == createdGroup.Id &&
                    e.Group.ChatName == dto.GroupName &&
                    e.Group.OwnerId == userId &&
                    e.UserId == userId));
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedGroupDto_WhenGroupCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateGroupDto
        {
            GroupName = "Test Group"
        };

        var createdGroup = new GroupChat
        {
            Id = Guid.NewGuid(),
            ChatName = dto.GroupName,
            Description = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        createdGroup.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = createdGroup.CreatedAt,
            GroupId = createdGroup.Id
        });

        var groupDto = new GroupDto
        {
            Id = createdGroup.Id,
            ChatName = dto.GroupName,
            OwnerId = userId,
            CreatedAt = createdGroup.CreatedAt
        };

        _groupRepository
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .CreateAsync(
                Arg.Any<GroupChat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdGroup);

        _mapper
            .Map<GroupDto>(createdGroup)
            .Returns(groupDto);

        var command = new CreateGroupCommand(dto, userId);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().Be(groupDto);
        result.Should().NotBeNull();
        result.OwnerId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_ShouldSetGroupProperties_WhenCreatingGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateGroupDto
        {
            GroupName = "My New Group"
        };

        var createdGroup = new GroupChat
        {
            Id = Guid.NewGuid(),
            ChatName = dto.GroupName,
            Description = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        createdGroup.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            MemberId = userId,
            GroupRole = GroupRole.Owner,
            CreatedAt = createdGroup.CreatedAt,
            GroupId = createdGroup.Id
        });

        var groupDto = new GroupDto
        {
            Id = createdGroup.Id,
            ChatName = dto.GroupName,
            OwnerId = userId,
            CreatedAt = createdGroup.CreatedAt
        };

        _groupRepository
            .GetTotalCountOwnedAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(0);

        _groupRepository
            .CreateAsync(
                Arg.Any<GroupChat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdGroup);

        _mapper
            .Map<GroupDto>(createdGroup)
            .Returns(groupDto);

        var command = new CreateGroupCommand(dto, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<GroupChat>(g =>
                    g.ChatName == "My New Group" &&
                    g.Description == null &&
                    g.ImageUrl == null &&
                    g.OwnerId == userId &&
                    g.Id != Guid.Empty &&
                    g.CreatedAt != default),
                Arg.Any<CancellationToken>());
    }
}