using FluentValidation;
using VOID.API.Endpoints.Messages;
using VOID.Shared.Contracts.Enums.Messages;

namespace VOID.API.Validators.Messages;

public class CreateMessageRequestValidator : AbstractValidator<CreateMessageRequest>
{
    public CreateMessageRequestValidator()
    {
        RuleFor(x => x.ChatType)
            .IsInEnum().WithMessage("Некорректный тип чата");

        RuleFor(x => x.MessageType)
            .IsInEnum().WithMessage("Некорректный тип сообщения");

        RuleFor(x => x.ParentId)
            .NotEmpty()
            .WithMessage("ParentId не может быть пустым")
            .NotEqual(Guid.Empty).WithMessage("ParentId не может быть Guid.Empty");

        When(x => x.MessageType == MessageType.Text, () =>
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Текст сообщения не может быть пустым")
                .MaximumLength(4000).WithMessage("Текст сообщения не должен превышать 4000 символов");

            RuleFor(x => x.Media)
                .Null().WithMessage("Текстовые сообщения не могут содержать медиафайлы");
        });

        When(x => x.MessageType == MessageType.Image, () =>
        {
            RuleFor(x => x)
                .Must(x => x.Media != null).WithMessage("Изображение обязательно");

            RuleFor(x => x.Text)
                .Empty().WithMessage("Медиа сообщения не могут содержать текст");
        });

        When(x => x.MessageType == MessageType.Gif, () =>
        {
            RuleFor(x => x)
                .Must(x => x.Media != null).WithMessage("Гифка обязательна");
            
            RuleFor(x => x.Text)
                .Null().WithMessage("Медиа сообщения не могут содержать текст");
        });
    }
}