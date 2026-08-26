using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Users.Commands.Connect;
using VOID.Application.UseCases.Users.Events.Connections;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Users.Commands.Connect;

public sealed class UserConnectedCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageBus _bus;

    private readonly UserConnectedCommandHandler _sut;

    public UserConnectedCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _bus = Substitute.For<IMessageBus>();

        _sut = new UserConnectedCommandHandler(
            _userRepository,
            _bus);
    }

    [Fact]
    public async Task Handle_ShouldChangeOnlineStatusToTrue_WhenUserConnects()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new UserConnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                true);
    }

    [Fact]
    public async Task Handle_ShouldPublishUserStatusChangedEvent_WhenUserConnects()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new UserConnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.UserId == userId &&
                    e.Status == true));
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserId_WhenChangingStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var command = new UserConnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                true);

        await _userRepository
            .DidNotReceive()
            .OnlineStatusChangeAsync(
                differentUserId,
                true);
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectUserId_WhenUserConnects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var command = new UserConnectedCommand(userId);

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
    public async Task Handle_ShouldSetIsOnlineToTrue_WhenPublishingEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new UserConnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.Status == true));
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenRepositoryAndBusComplete()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                true)
            .Returns(Task.CompletedTask);

        // Не настраиваем bus - он вернет ValueTask.CompletedTask по умолчанию

        var command = new UserConnectedCommand(userId);

        // Act
        var act = () => _sut.Handle(command);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedException = new Exception("Database error");

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                true)
            .Returns(Task.FromException(expectedException));

        var command = new UserConnectedCommand(userId);

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
    public async Task Handle_ShouldCallRepositoryBeforeBus_WhenUserConnects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var callOrder = new List<string>();

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                true)
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("repository"));

        _bus
            .When(x => x.PublishAsync(Arg.Any<UserStatusChangedEvent>()))
            .Do(_ => callOrder.Add("bus"));

        var command = new UserConnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        callOrder.Should().ContainInOrder("repository", "bus");
    }

    [Fact]
    public async Task Handle_ShouldWorkWithEmptyGuid_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;

        var command = new UserConnectedCommand(userId);

        // Act
        await _sut.Handle(command);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                Guid.Empty,
                true);

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<UserStatusChangedEvent>(e =>
                    e.UserId == Guid.Empty &&
                    e.Status == true));
    }
}