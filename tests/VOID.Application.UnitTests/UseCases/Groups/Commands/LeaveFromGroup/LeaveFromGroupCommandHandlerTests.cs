using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Commands.LeaveFromGroup;
using VOID.Application.UseCases.Groups.Events.Leaved;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.Commands.LeaveFromGroup;

public sealed class LeaveFromGroupCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMessageBus _bus;

    private readonly LeaveFromGroupCommandHandler _sut;

    public LeaveFromGroupCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new LeaveFromGroupCommandHandler(
            _groupRepository,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new LeaveFromGroupCommand(groupId, userId);

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

        await _groupRepository
            .DidNotReceive()
            .IsOwnerAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteMemberAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<LeavedFromGroupEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserIsNotMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

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

        var command = new LeaveFromGroupCommand(groupId, userId);

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
            .Received(1)
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .IsOwnerAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteMemberAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<LeavedFromGroupEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenUserIsOwner()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new LeaveFromGroupCommand(groupId, userId);

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
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .IsMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteMemberAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<LeavedFromGroupEvent>());
    }

    [Fact]
    public async Task Handle_ShouldRemoveUserFromGroup_WhenAllValidationsPass()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new LeaveFromGroupCommand(groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .DeleteMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<LeavedFromGroupEvent>(e =>
                    e.GroupId == groupId &&
                    e.UserId == userId));
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenUserLeavesGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new LeaveFromGroupCommand(groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<LeavedFromGroupEvent>(e =>
                    e.GroupId == groupId &&
                    e.UserId == userId));
    }

    [Fact]
    public async Task Handle_ShouldRemoveCorrectUser_WhenLeavingGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new LeaveFromGroupCommand(groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .DeleteMemberAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteMemberAsync(
                groupId,
                differentUserId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotPublishEvent_WhenValidationsFail()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new LeaveFromGroupCommand(groupId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<LeavedFromGroupEvent>());
    }
}