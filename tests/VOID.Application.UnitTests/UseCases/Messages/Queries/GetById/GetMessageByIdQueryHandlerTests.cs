using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.UseCases.Messages.Queries.GetById;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Messages.Queries.GetById;

public sealed class GetMessageByIdQueryHandlerTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    private readonly GetMessageByIdQueryHandler _sut;

    public GetMessageByIdQueryHandlerTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetMessageByIdQueryHandler(
            _messageRepository,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnMessageDto_WhenMessageExists()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            Text = "Hello",
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            SenderId = userId,
            Text = "Hello",
            IsMine = true
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _mapper
            .Map<MessageDto>(
                message,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var query = new GetMessageByIdQuery(messageId, userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(messageDto);
        result.Id.Should().Be(messageId);

        await _messageRepository
            .Received(1)
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<MessageDto>(
                message,
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenMessageDoesNotExist()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        // Не настраиваем mapper, так как он вернет null по умолчанию
        // или настраиваем с Arg.Any<Message>()
        _mapper
            .Map<MessageDto>(
                Arg.Any<Message>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns((MessageDto?)null!);

        var query = new GetMessageByIdQuery(messageId, userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _messageRepository
            .Received(1)
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>());

        _mapper
            .Received(1)
            .Map<MessageDto>(
                Arg.Any<Message>(),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectMessageId_WhenGettingMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentMessageId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            Text = "Hello",
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            SenderId = userId,
            Text = "Hello"
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _mapper
            .Map<MessageDto>(
                message,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var query = new GetMessageByIdQuery(messageId, userId);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _messageRepository
            .Received(1)
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>());

        await _messageRepository
            .DidNotReceive()
            .GetByIdAsync(differentMessageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapWithCurrentUserId_WhenGettingMessage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            Text = "Hello",
            CreatedAt = DateTime.UtcNow
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            SenderId = userId,
            Text = "Hello",
            IsMine = true
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _mapper
            .Map<MessageDto>(
                message,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var query = new GetMessageByIdQuery(messageId, userId);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _mapper
            .Received(1)
            .Map<MessageDto>(
                Arg.Is<Message>(m => m.Id == messageId),
                Arg.Any<Action<IMappingOperationOptions>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnMessageWithCorrectProperties_WhenMessageExists()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            Text = "Test message",
            CreatedAt = DateTime.UtcNow,
            IsEdited = false
        };

        var messageDto = new MessageDto
        {
            Id = messageId,
            SenderId = userId,
            Text = "Test message",
            IsMine = true,
            IsEdited = false
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _mapper
            .Map<MessageDto>(
                message,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(messageDto);

        var query = new GetMessageByIdQuery(messageId, userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(messageId);
        result.SenderId.Should().Be(userId);
        result.Text.Should().Be("Test message");
        result.IsMine.Should().BeTrue();
        result.IsEdited.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenMapperReturnsNull()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            SenderId = userId,
            Text = "Hello",
            CreatedAt = DateTime.UtcNow
        };

        _messageRepository
            .GetByIdAsync(messageId, Arg.Any<CancellationToken>())
            .Returns(message);

        _mapper
            .Map<MessageDto>(
                message,
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns((MessageDto?)null!);

        var query = new GetMessageByIdQuery(messageId, userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mapper
            .Received(1)
            .Map<MessageDto>(
                message,
                Arg.Any<Action<IMappingOperationOptions>>());
    }
}