using FluentValidation;
using HobbyApp.Application.DTOs;

namespace HobbyApp.Application.Validation;

public class RoleDtoValidator : AbstractValidator<RoleDto>
{
    public RoleDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .Length(2, 50).WithMessage("Role name must be between 2 and 50 characters")
            .Matches("^[a-zA-Z0-9\\s_-]+$").WithMessage("Role name can only contain letters, numbers, spaces, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
