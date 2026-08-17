using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Users.Commands.ChangeOnline;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Users.Commands.ChangeOnline;

public sealed class ChangeOnlineCommandHandlerTests
{
    private readonly IUserRepository _userRepository;

    private readonly ChangeOnlineCommandHandler _sut;

    public ChangeOnlineCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();

        _sut = new ChangeOnlineCommandHandler(
            _userRepository);
    }

    [Fact]
    public async Task HandleAsync_ShouldChangeOnlineStatus_WhenCalled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var isOnline = true;

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                isOnline,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectUserId_WhenChangingOnlineStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var isOnline = true;

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                isOnline,
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .OnlineStatusChangeAsync(
                differentUserId,
                isOnline,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectIsOnlineValue_WhenTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var isOnline = true;

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                true,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectIsOnlineValue_WhenFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var isOnline = false;

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                false,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectCancellationToken_WhenChangingOnlineStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var isOnline = true;
        var ct = new CancellationTokenSource().Token;

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        await _sut.HandleAsync(command, ct);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                userId,
                isOnline,
                ct);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotThrow_WhenRepositoryCompletes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var isOnline = true;

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                isOnline,
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var isOnline = true;
        var expectedException = new Exception("Database error");

        _userRepository
            .OnlineStatusChangeAsync(
                userId,
                isOnline,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(expectedException));

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryOnlyOnce_WhenChangingOnlineStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var isOnline = true;

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldWorkWithEmptyGuid_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;
        var isOnline = false;

        var command = new ChangeOnlineCommand(userId, isOnline);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .OnlineStatusChangeAsync(
                Guid.Empty,
                false,
                Arg.Any<CancellationToken>());
    }
}