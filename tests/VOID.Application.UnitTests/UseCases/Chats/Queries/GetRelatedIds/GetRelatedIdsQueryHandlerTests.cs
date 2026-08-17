using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.UseCases.Chats.Queries.GetRelatedIds;
using VOID.Shared.Contracts.DTOs.Paginations;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Chats.Queries.GetRelatedIds;

public sealed class GetRelatedIdsQueryHandlerTests
{
    private readonly IChatRepository _chatRepository;

    private readonly GetRelatedIdsQueryHandler _sut;

    public GetRelatedIdsQueryHandlerTests()
    {
        _chatRepository = Substitute.For<IChatRepository>();

        _sut = new GetRelatedIdsQueryHandler(
            _chatRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnRelatedUserIds_WhenUsersExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var relatedUserId1 = Guid.NewGuid();
        var relatedUserId2 = Guid.NewGuid();
        var relatedUserId3 = Guid.NewGuid();

        var expectedIds = new List<Guid>
        {
            relatedUserId1,
            relatedUserId2,
            relatedUserId3
        };

        _chatRepository
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(expectedIds);

        var query = new GetRelatedIdsQuery(userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(expectedIds);

        await _chatRepository
            .Received(1)
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRelatedUsersExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _chatRepository
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetRelatedIdsQuery(userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        await _chatRepository
            .Received(1)
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _chatRepository
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns((List<Guid>?)null!);

        var query = new GetRelatedIdsQuery(userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _chatRepository
            .Received(1)
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserId_WhenGettingRelatedIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var relatedUserId = Guid.NewGuid();

        _chatRepository
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns([relatedUserId]);

        var query = new GetRelatedIdsQuery(userId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotDuplicateIds_WhenRepositoryReturnsUniqueIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var relatedUserId1 = Guid.NewGuid();
        var relatedUserId2 = Guid.NewGuid();

        var expectedIds = new List<Guid>
        {
            relatedUserId1,
            relatedUserId2
        };

        _chatRepository
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(expectedIds);

        var query = new GetRelatedIdsQuery(userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().OnlyHaveUniqueItems();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnIdsInCorrectOrder_WhenRepositoryReturnsOrderedIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();

        var expectedIds = new List<Guid>
        {
            firstId,
            secondId,
            thirdId
        };

        _chatRepository
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(expectedIds);

        var query = new GetRelatedIdsQuery(userId);

        // Act
        var result = await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        result[0].Should().Be(firstId);
        result[1].Should().Be(secondId);
        result[2].Should().Be(thirdId);
    }

    [Fact]
    public async Task Handle_ShouldNotCallOtherRepositoryMethods_WhenGettingRelatedIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var relatedUserId = Guid.NewGuid();

        _chatRepository
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns([relatedUserId]);

        var query = new GetRelatedIdsQuery(userId);

        // Act
        await _sut.Handle(
            query,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .GetRelatedUsersIdsAsync(
                userId,
                Arg.Any<CancellationToken>());

        // Проверяем, что другие методы репозитория не вызывались
        await _chatRepository
            .DidNotReceive()
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .GetAllByUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<PaginationRequest>(),
                Arg.Any<CancellationToken>());
    }
}