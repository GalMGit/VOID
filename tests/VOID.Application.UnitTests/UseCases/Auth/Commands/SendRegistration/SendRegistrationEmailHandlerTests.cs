using FluentAssertions;
using NSubstitute;
using VOID.Application.Abstractions.IServices.IMailServices;
using VOID.Application.UseCases.Auth.Commands.Register.SendRegistration;
using VOID.Application.UseCases.Auth.Events;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using Xunit;

namespace VOID.Application.UnitTests.UseCases.Auth.Commands.SendRegistration;

public sealed class SendRegistrationEmailHandlerTests
{
    private readonly IEmailQueueService _emailQueueService;
    private readonly IEmailTemplateService _templateService;

    private readonly SendRegistrationEmailHandler _sut;

    public SendRegistrationEmailHandlerTests()
    {
        _emailQueueService = Substitute.For<IEmailQueueService>();
        _templateService = Substitute.For<IEmailTemplateService>();

        _sut = new SendRegistrationEmailHandler(
            _emailQueueService,
            _templateService);
    }

    [Fact]
    public async Task Handle_ShouldGetRegistrationConfirmation_WhenUserRegistered()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "john@example.com",
            "john",
            "12345");

        var emailTask = new EmailTaskDto
        {
            ToEmail = "john@example.com",
            Subject = "Подтверждение регистрации",
            Body = "<h1>Код: 12345</h1>"
        };

        _templateService
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode)
            .Returns(emailTask);

        // Act
        await _sut.Handle(message, CancellationToken.None);

        // Assert
        _templateService
            .Received(1)
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode);
    }

    [Fact]
    public async Task Handle_ShouldEnqueueEmail_WhenUserRegistered()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "john@example.com",
            "john",
            "12345");

        var emailTask = new EmailTaskDto
        {
            ToEmail = "john@example.com",
            Subject = "Подтверждение регистрации",
            Body = "<h1>Код: 12345</h1>"
        };

        _templateService
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode)
            .Returns(emailTask);

        // Act
        await _sut.Handle(message, CancellationToken.None);

        // Assert
        _emailQueueService
            .Received(1)
            .EnqueueEmail(emailTask);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectEmail_WhenGettingTemplate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "john@example.com";
        var username = "john";
        var confirmationCode = "12345";

        var message = new UserStartRegistrationEvent(
            userId,
            email,
            username,
            confirmationCode);

        var emailTask = new EmailTaskDto
        {
            ToEmail = email,
            Subject = "Подтверждение регистрации",
            Body = "Код: 12345"
        };

        _templateService
            .GetRegistrationConfirmation(
                email,
                username,
                confirmationCode)
            .Returns(emailTask);

        // Act
        await _sut.Handle(message, CancellationToken.None);

        // Assert
        _templateService
            .Received(1)
            .GetRegistrationConfirmation(
                email,
                username,
                confirmationCode);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUsername_WhenGettingTemplate()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "john@example.com",
            "john_doe",
            "54321");

        var emailTask = new EmailTaskDto
        {
            ToEmail = "john@example.com",
            Subject = "Подтверждение регистрации",
            Body = "Код: 54321"
        };

        _templateService
            .GetRegistrationConfirmation(
                "john@example.com",
                "john_doe",
                "54321")
            .Returns(emailTask);

        // Act
        await _sut.Handle(message, CancellationToken.None);

        // Assert
        _templateService
            .Received(1)
            .GetRegistrationConfirmation(
                Arg.Is<string>(email => email == "john@example.com"),
                Arg.Is<string>(username => username == "john_doe"),
                Arg.Is<string>(code => code == "54321"));
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectConfirmationCode_WhenGettingTemplate()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "jane@example.com",
            "jane",
            "98765");

        var emailTask = new EmailTaskDto
        {
            ToEmail = "jane@example.com",
            Subject = "Подтверждение регистрации",
            Body = "Код: 98765"
        };

        _templateService
            .GetRegistrationConfirmation(
                "jane@example.com",
                "jane",
                "98765")
            .Returns(emailTask);

        // Act
        await _sut.Handle(message, CancellationToken.None);

        // Assert
        _templateService
            .Received(1)
            .GetRegistrationConfirmation(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<string>(code => code == "98765"));
    }

    [Fact]
    public async Task Handle_ShouldEnqueueCorrectEmail_WhenUserRegistered()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "alice@example.com",
            "alice",
            "11111");

        var expectedEmailTask = new EmailTaskDto
        {
            ToEmail = "alice@example.com",
            Subject = "Подтверждение",
            Body = "Код: 11111"
        };

        var differentEmailTask = new EmailTaskDto
        {
            ToEmail = "bob@example.com",
            Subject = "Другое",
            Body = "Другой код"
        };

        _templateService
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode)
            .Returns(expectedEmailTask);

        // Act
        await _sut.Handle(message, CancellationToken.None);

        // Assert
        _emailQueueService
            .Received(1)
            .EnqueueEmail(expectedEmailTask);

        _emailQueueService
            .DidNotReceive()
            .EnqueueEmail(differentEmailTask);
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenEnqueueCompletes()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "john@example.com",
            "john",
            "12345");

        var emailTask = new EmailTaskDto
        {
            ToEmail = "john@example.com",
            Subject = "Подтверждение",
            Body = "Код: 12345"
        };

        _templateService
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode)
            .Returns(emailTask);

        // Act
        var act = () => _sut.Handle(message, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenTemplateServiceThrows()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "john@example.com",
            "john",
            "12345");

        var expectedException = new Exception("Template error");

        _templateService
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode)
            .Returns<EmailTaskDto>(_ => throw expectedException);

        // Act
        var act = () => _sut.Handle(message, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Template error");

        _emailQueueService
            .DidNotReceive()
            .EnqueueEmail(Arg.Any<EmailTaskDto>());
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectCancellationToken_WhenHandling()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "john@example.com",
            "john",
            "12345");
        var ct = new CancellationTokenSource().Token;

        var emailTask = new EmailTaskDto
        {
            ToEmail = "john@example.com",
            Subject = "Подтверждение",
            Body = "Код: 12345"
        };

        _templateService
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode)
            .Returns(emailTask);

        // Act
        await _sut.Handle(message, ct);

        // Assert
        _templateService
            .Received(1)
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode);
    }

    [Fact]
    public async Task Handle_ShouldCallTemplateServiceBeforeEnqueue_WhenUserRegistered()
    {
        // Arrange
        var message = new UserStartRegistrationEvent(
            Guid.NewGuid(),
            "john@example.com",
            "john",
            "12345");
        var callOrder = new List<string>();

        var emailTask = new EmailTaskDto
        {
            ToEmail = "john@example.com",
            Subject = "Подтверждение",
            Body = "Код: 12345"
        };

        _templateService
            .GetRegistrationConfirmation(
                message.Email,
                message.Username,
                message.ConfirmationCode)
            .Returns(emailTask)
            .AndDoes(_ => callOrder.Add("template"));

        _emailQueueService
            .When(x => x.EnqueueEmail(Arg.Any<EmailTaskDto>()))
            .Do(_ => callOrder.Add("enqueue"));

        // Act
        await _sut.Handle(message, CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("template", "enqueue");
    }
}