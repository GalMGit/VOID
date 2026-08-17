using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Commands.Delete;
using VOID.Application.UseCases.Groups.Events.Deleted;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.Commands.Delete;

public sealed class DeleteGroupCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMessageBus _bus;

    private readonly DeleteGroupCommandHandler _sut;

    public DeleteGroupCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new DeleteGroupCommandHandler(
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

        var command = new DeleteGroupCommand(groupId, userId);

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
            .IsOwnerAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<GroupDeletedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotOwner()
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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteGroupCommand(groupId, userId);

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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<GroupDeletedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteGroup_WhenUserIsOwner()
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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteGroupCommand(groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .DeleteAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupDeletedEvent>(e =>
                    e.GroupId == groupId));
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectGroupId_WhenGroupDeleted()
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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteGroupCommand(groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<GroupDeletedEvent>(e =>
                    e.GroupId == groupId));
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteGroup_WhenUserIsNotOwner()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid(); // Другой пользователь - владелец

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteGroupCommand(groupId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ForbiddenException>();

        await _groupRepository
            .DidNotReceive()
            .DeleteAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Is<GroupDeletedEvent>(e =>
                    e.GroupId == groupId));
    }

    [Fact]
    public async Task Handle_ShouldCheckOwnership_WhenGroupExists()
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
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteGroupCommand(groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteGroupWithCorrectId_WhenUserIsOwner()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentGroupId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteGroupCommand(groupId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _groupRepository
            .Received(1)
            .DeleteAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteAsync(
                differentGroupId,
                Arg.Any<CancellationToken>());
    }
}