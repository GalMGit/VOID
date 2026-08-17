using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Users.Commands.Disconnect;
using VOID.Application.UseCases.Users.Events.Connections;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Users.Commands.Disconnect;

public sealed class UserDisconnectedCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageBus _bus;

    private readonly UserDisconnectedCommandHandler _sut;

    public UserDisconnectedCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new UserDisconnectedCommandHandler(
            _userRepository,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldChangeOnlineStatusToFalse_WhenUserDisconnects()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                false);
    }

    [Fact]
    public async Task Handle_ShouldChangeLastSeen_WhenUserDisconnects()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                userId);
    }

    [Fact]
    public async Task Handle_ShouldPublishUserStatusChangedEvent_WhenUserDisconnects()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.UserId == userId &&
                    e.Status == false));
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserId_WhenChangingStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                false);

        await _userRepository
            .DidNotReceive()
            .OnlineStatusChangeAsync(
                differentUserId,
                false);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserId_WhenChangingLastSeen()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                userId);

        await _userRepository
            .DidNotReceive()
            .ChangeUserLastSeenAsync(
                differentUserId);
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectUserId_WhenUserDisconnects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.UserId == userId));

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.UserId == differentUserId));
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToFalse_WhenPublishingEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.Status == false));
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenRepositoryAndBusComplete()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                false)
            .Returns(Task.CompletedTask);

        _userRepository
            .ChangeUserLastSeenAsync(
                userId)
            .Returns(Task.CompletedTask);

        var command = new UserDisconnectedCommand(userId);

        // Act
        var act = () => _sut.Handle(command);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenOnlineStatusChangeThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedException = new Exception("Database error");

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                false)
            .Returns(Task.FromException(expectedException));

        var command = new UserDisconnectedCommand(userId);

        // Act
        var act = () => _sut.Handle(command);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");

        await _userRepository
            .DidNotReceive()
            .ChangeUserLastSeenAsync(Arg.Any<Guid>());

        await _bus
            .DidNotReceive()
            .PublishAsync(Arg.Any<UserStatusChangedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenChangeLastSeenThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedException = new Exception("Database error");

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                false)
            .Returns(Task.CompletedTask);

        _userRepository
            .ChangeUserLastSeenAsync(
                userId)
            .Returns(Task.FromException(expectedException));

        var command = new UserDisconnectedCommand(userId);

        // Act
        var act = () => _sut.Handle(command);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");

        await _bus
            .DidNotReceive()
            .PublishAsync(Arg.Any<UserStatusChangedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldCallMethodsInCorrectOrder_WhenUserDisconnects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var callOrder = new List<string>();

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                false)
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("onlineStatus"));

        _userRepository
            .ChangeUserLastSeenAsync(
                userId)
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("lastSeen"));

        _bus
            .When(x => x.PublishAsync(Arg.Any<UserStatusChangedEvent>()))
            .Do(_ => callOrder.Add("bus"));

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        callOrder.Should().ContainInOrder("onlineStatus", "lastSeen", "bus");
    }

    [Fact]
    public async Task Handle_ShouldWorkWithEmptyGuid_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;

        var command = new UserDisconnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                Guid.Empty,
                false);

        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                Guid.Empty);

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.UserId == Guid.Empty &&
                    e.Status == false));
    }
}