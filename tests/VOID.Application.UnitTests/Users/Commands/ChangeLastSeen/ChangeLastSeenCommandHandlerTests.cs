using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Users.Commands.ChangeLastSeen;
using Xunit;

namespace VOID.Application.UnitTests.Users.Commands.ChangeLastSeen;

public sealed class ChangeLastSeenCommandHandlerTests
{
    private readonly IUserRepository _userRepository;

    private readonly ChangeLastSeenCommandHandler _sut;

    public ChangeLastSeenCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();

        _sut = new ChangeLastSeenCommandHandler(
            _userRepository);
    }

    [Fact]
    public async Task HandleAsync_ShouldChangeLastSeen_WhenCalled()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new ChangeLastSeenCommand(userId);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectUserId_WhenChangingLastSeen()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var command = new ChangeLastSeenCommand(userId);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                userId,
                Arg.Any<CancellationToken>());

        await _userRepository
            .DidNotReceive()
            .ChangeUserLastSeenAsync(
                differentUserId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectCancellationToken_WhenChangingLastSeen()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ct = new CancellationTokenSource().Token;

        var command = new ChangeLastSeenCommand(userId);

        // Act
        await _sut.HandleAsync(command, ct);

        // Assert
        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                userId,
                ct);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotThrow_WhenRepositoryCompletes()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepository
            .ChangeUserLastSeenAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new ChangeLastSeenCommand(userId);

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
        var expectedException = new Exception("Database error");

        _userRepository
            .ChangeUserLastSeenAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(expectedException));

        var command = new ChangeLastSeenCommand(userId);

        // Act
        var act = () => _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryOnlyOnce_WhenChangingLastSeen()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new ChangeLastSeenCommand(userId);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldWorkWithEmptyGuid_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;

        var command = new ChangeLastSeenCommand(userId);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository
            .Received(1)
            .ChangeUserLastSeenAsync(
                Guid.Empty,
                Arg.Any<CancellationToken>());
    }
}