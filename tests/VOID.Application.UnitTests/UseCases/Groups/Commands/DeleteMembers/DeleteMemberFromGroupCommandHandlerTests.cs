using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Commands.DeleteMembers;
using VOID.Application.UseCases.Groups.Events.MemberDeleted;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Groups.Commands.DeleteMembers;

public sealed class DeleteMemberFromGroupCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMessageBus _bus;

    private readonly DeleteMemberFromGroupCommandHandler _sut;

    public DeleteMemberFromGroupCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new DeleteMemberFromGroupCommandHandler(
            _groupRepository);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteMemberFromGroupCommand(groupId, memberId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None,
            _bus);

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
                Arg.Any<MemberDeletedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteMemberFromGroupCommand(groupId, memberId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None,
            _bus);

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
                memberId,
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
                Arg.Any<MemberDeletedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteMemberFromGroupCommand(groupId, memberId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None,
            _bus);

        // Assert
        await act
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();

        await _groupRepository
            .Received(1)
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .Received(1)
            .IsMemberAsync(
                groupId,
                memberId,
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
                Arg.Any<MemberDeletedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenDeletingOwner()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid(); // Другой владелец

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                ownerId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                ownerId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteMemberFromGroupCommand(groupId, ownerId, userId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None,
            _bus);

        // Assert
        await act
            .Should()
            .ThrowAsync<ValidationException>();

        await _groupRepository
            .Received(1)
            .IsOwnerAsync(
                groupId,
                ownerId,
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
                Arg.Any<MemberDeletedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteMember_WhenAllValidationsPass()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteMemberFromGroupCommand(groupId, memberId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None,
            _bus);

        // Assert
        await _groupRepository
            .Received(1)
            .DeleteMemberAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<MemberDeletedEvent>(e =>
                    e.GroupId == groupId &&
                    e.MemberId == memberId &&
                    e.UserId == userId));
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData_WhenMemberDeleted()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteMemberFromGroupCommand(groupId, memberId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None,
            _bus);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<MemberDeletedEvent>(e =>
                    e.GroupId == groupId &&
                    e.MemberId == memberId &&
                    e.UserId == userId));
    }

    [Fact]
    public async Task Handle_ShouldDeleteCorrectMember_WhenDeletingMember()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentMemberId = Guid.NewGuid();

        _groupRepository
            .ExistsAsync(
                groupId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsMemberAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                userId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _groupRepository
            .IsOwnerAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteMemberFromGroupCommand(groupId, memberId, userId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None,
            _bus);

        // Assert
        await _groupRepository
            .Received(1)
            .DeleteMemberAsync(
                groupId,
                memberId,
                Arg.Any<CancellationToken>());

        await _groupRepository
            .DidNotReceive()
            .DeleteMemberAsync(
                groupId,
                differentMemberId,
                Arg.Any<CancellationToken>());
    }
}