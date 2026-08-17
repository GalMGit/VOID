using FluentValidation;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.API.Validators.Groups;

public class CreateGroupValidator : AbstractValidator<CreateGroupDto>
{
    public CreateGroupValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Название группы не может быть пустым")
            .MaximumLength(15).WithMessage("Название группы не может быть больше 15 символов");
    }
}
