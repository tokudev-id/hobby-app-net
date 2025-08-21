using FluentValidation;
using HobbyApp.Application.DTOs;

namespace HobbyApp.Application.Validation;

public class UpdateUserCommandDtoValidator : AbstractValidator<UpdateUserCommandDto>
{
    public UpdateUserCommandDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .Length(5, 255).WithMessage("Email must be between 5 and 255 characters");

        RuleForEach(x => x.Hobbies)
            .SetValidator(new HobbyItemDtoValidator());
    }
}

