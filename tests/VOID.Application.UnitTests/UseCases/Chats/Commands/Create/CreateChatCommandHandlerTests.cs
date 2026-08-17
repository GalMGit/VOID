using AutoMapper;
using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Chats.Commands.Create;
using VOID.Application.UseCases.Chats.Events.Created;
using VOID.Domain.Models.Chats;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Chats;
using Wolverine;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Chats.Commands.Create;

public sealed class CreateChatCommandHandlerTests
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageBus _bus;
    private readonly IMapper _mapper;

    private readonly CreateChatCommandHandler _sut;

    public CreateChatCommandHandlerTests()
    {
        _chatRepository = Substitute.For<IChatRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _bus = Substitute.For<IMessageBus>();
        _mapper = Substitute.For<IMapper>();

        _sut = new CreateChatCommandHandler(
            _chatRepository,
            _userRepository,
            _bus,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTargetUserDoesNotExist()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var dto = new CreateChatDto
        {
            Username = "nonexistent"
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new CreateChatCommand(dto, currentUserId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>();

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .ExistsBetweenUsersAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<Chat>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<ChatCreatedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenCreatingChatWithSelf()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var dto = new CreateChatDto
        {
            Username = "john"
        };

        var targetUser = new User
        {
            Id = currentUserId,
            Username = dto.Username,
            Email = "john@example.com"
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(targetUser);

        var command = new CreateChatCommand(dto, currentUserId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .ExistsBetweenUsersAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<Chat>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<ChatCreatedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenChatAlreadyExists()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var dto = new CreateChatDto
        {
            Username = "target"
        };

        var targetUser = new User
        {
            Id = targetUserId,
            Username = dto.Username,
            Email = "target@example.com"
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(targetUser);

        _chatRepository
            .ExistsBetweenUsersAsync(
                currentUserId,
                targetUserId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateChatCommand(dto, currentUserId);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<ConflictException>();

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .ExistsBetweenUsersAsync(
                currentUserId,
                targetUserId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .DidNotReceive()
            .CreateAsync(
                Arg.Any<Chat>(),
                Arg.Any<CancellationToken>());

        await _bus
            .DidNotReceive()
            .PublishAsync(
                Arg.Any<ChatCreatedEvent>());
    }

    [Fact]
    public async Task Handle_ShouldCreateChatWithCorrectData_WhenValidationsPass()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var dto = new CreateChatDto
        {
            Username = "target"
        };

        var targetUser = new User
        {
            Id = targetUserId,
            Username = dto.Username,
            Email = "target@example.com"
        };

        var createdChat = new Chat
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatorId = currentUserId,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(targetUser);

        _chatRepository
            .ExistsBetweenUsersAsync(
                currentUserId,
                targetUserId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _chatRepository
            .CreateAsync(
                Arg.Any<Chat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdChat);

        var creatorChatDto = new ChatDto { Id = createdChat.Id };
        var recipientChatDto = new ChatDto { Id = createdChat.Id };

        // Настраиваем mapper для двух вызовов
        _mapper
            .Map<ChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(creatorChatDto, recipientChatDto);

        var command = new CreateChatCommand(dto, currentUserId);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(creatorChatDto);

        await _userRepository
            .Received(1)
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .ExistsBetweenUsersAsync(
                currentUserId,
                targetUserId,
                Arg.Any<CancellationToken>());

        await _chatRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<Chat>(chat =>
                    chat.Id != Guid.Empty &&
                    chat.CreatorId == currentUserId &&
                    chat.Interlocutors.Count == 2 &&
                    chat.Interlocutors.Any(i => i.UserId == currentUserId) &&
                    chat.Interlocutors.Any(i => i.UserId == targetUserId)),
                Arg.Any<CancellationToken>());

        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<ChatCreatedEvent>(e =>
                    e.ChatId == createdChat.Id &&
                    e.CreatorId == currentUserId &&
                    e.RecipientId == targetUserId &&
                    e.CreatorChat == creatorChatDto &&
                    e.RecipientChat == recipientChatDto));
    }

    [Fact]
    public async Task Handle_ShouldCreateChatWithTwoInterlocutors_WhenValidationsPass()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var dto = new CreateChatDto
        {
            Username = "target"
        };

        var targetUser = new User
        {
            Id = targetUserId,
            Username = dto.Username,
            Email = "target@example.com"
        };

        var createdChat = new Chat
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatorId = currentUserId,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(targetUser);

        _chatRepository
            .ExistsBetweenUsersAsync(
                currentUserId,
                targetUserId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _chatRepository
            .CreateAsync(
                Arg.Any<Chat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdChat);

        var creatorChatDto = new ChatDto { Id = createdChat.Id };
        var recipientChatDto = new ChatDto { Id = createdChat.Id };

        _mapper
            .Map<ChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(creatorChatDto, recipientChatDto);

        var command = new CreateChatCommand(dto, currentUserId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _chatRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<Chat>(chat =>
                    chat.Interlocutors.Count == 2 &&
                    chat.Interlocutors.Any(i => i.UserId == currentUserId) &&
                    chat.Interlocutors.Any(i => i.UserId == targetUserId)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectRecipientId_WhenChatCreated()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var dto = new CreateChatDto
        {
            Username = "target"
        };

        var targetUser = new User
        {
            Id = targetUserId,
            Username = dto.Username,
            Email = "target@example.com"
        };

        var createdChat = new Chat
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatorId = currentUserId,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(targetUser);

        _chatRepository
            .ExistsBetweenUsersAsync(
                currentUserId,
                targetUserId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _chatRepository
            .CreateAsync(
                Arg.Any<Chat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdChat);

        var creatorChatDto = new ChatDto { Id = createdChat.Id };
        var recipientChatDto = new ChatDto { Id = createdChat.Id };

        _mapper
            .Map<ChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(creatorChatDto, recipientChatDto);

        var command = new CreateChatCommand(dto, currentUserId);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await _bus
            .Received(1)
            .PublishAsync(
                Arg.Is<ChatCreatedEvent>(e =>
                    e.RecipientId == targetUserId &&
                    e.CreatorId == currentUserId));
    }

    [Fact]
    public async Task Handle_ShouldReturnCreatorChatDto_WhenChatCreated()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var dto = new CreateChatDto
        {
            Username = "target"
        };

        var targetUser = new User
        {
            Id = targetUserId,
            Username = dto.Username,
            Email = "target@example.com"
        };

        var createdChat = new Chat
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatorId = currentUserId,
            Interlocutors = new List<ChatInterlocutor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    ChatId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _userRepository
            .GetByUsernameAsync(
                dto.Username,
                Arg.Any<CancellationToken>())
            .Returns(targetUser);

        _chatRepository
            .ExistsBetweenUsersAsync(
                currentUserId,
                targetUserId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _chatRepository
            .CreateAsync(
                Arg.Any<Chat>(),
                Arg.Any<CancellationToken>())
            .Returns(createdChat);

        var creatorChatDto = new ChatDto { Id = createdChat.Id };
        var recipientChatDto = new ChatDto { Id = createdChat.Id };

        _mapper
            .Map<ChatDto>(
                Arg.Any<Chat>(),
                Arg.Any<Action<IMappingOperationOptions>>())
            .Returns(creatorChatDto, recipientChatDto);

        var command = new CreateChatCommand(dto, currentUserId);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().Be(creatorChatDto);
        result.Should().NotBe(recipientChatDto);
    }
}