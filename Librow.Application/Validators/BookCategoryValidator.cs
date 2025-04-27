using FluentValidation;
using Librow.Application.Models.Requests;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Validators;
public sealed class BookCategoryValidator : AbstractValidator<BookCategoryRequest>
{
    public BookCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name of BookCategory must not be empty!");
    }
}
