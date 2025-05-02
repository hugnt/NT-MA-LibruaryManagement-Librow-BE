using FluentValidation;
using Librow.Application.Models.Requests;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Validators;
public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
           .NotEmpty().WithMessage("Username must not be empty!");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password must not be empty!");
    }
}

public sealed class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Fullname)
            .NotEmpty().WithMessage("Fullname must not be empty!")
            .MaximumLength(50).WithMessage("Fullname must not exceed 50 characters!");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email must not be empty!")
            .EmailAddress().WithMessage("Invalid email format!");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username must not be empty!")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters!")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters!");

        RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password must not be empty!")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters!");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role value! Role must be one of the defined values (e.g., Customer, Admin).");

    }
}

public sealed class UserUpdateValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.Fullname)
            .NotEmpty().WithMessage("Fullname must not be empty!")
            .MaximumLength(50).WithMessage("Fullname must not exceed 50 characters!");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email must not be empty!")
            .EmailAddress().WithMessage("Invalid email format!");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username must not be empty!")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters!")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters!");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role value! Role must be one of the defined values (e.g., Customer, Admin).");

    }
}
