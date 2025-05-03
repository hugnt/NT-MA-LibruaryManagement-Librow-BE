using FluentValidation;
using Librow.Application.Models.Requests;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Validators;
public sealed class BookRatingValidator : AbstractValidator<BookRatingRequest>
{
    public BookRatingValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(255).WithMessage("Comment must not exceed 255 characters.");

        RuleFor(x => x.Rate)
            .GreaterThanOrEqualTo(0).WithMessage("Rate must be greater than or equal to 0.")
            .LessThanOrEqualTo(5).WithMessage("Rate must be lower than or equal to 5.");

    }
}
