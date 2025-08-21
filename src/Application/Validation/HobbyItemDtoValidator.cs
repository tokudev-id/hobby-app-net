using FluentValidation;
using HobbyApp.Application.DTOs;

namespace HobbyApp.Application.Validation;

public class HobbyItemDtoValidator : AbstractValidator<HobbyItemDto>
{
    public HobbyItemDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hobby name is required")
            .Length(2, 100).WithMessage("Hobby name must be between 2 and 100 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Hobby name cannot be empty or contain only whitespace");

        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Invalid hobby level. Must be Beginner, Intermediate, or Expert");
    }
}
