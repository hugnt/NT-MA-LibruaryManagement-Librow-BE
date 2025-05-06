using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Librow.Application.Models;
using System.Net;
using System.Diagnostics.CodeAnalysis;

namespace Librow.API.Filters;

[ExcludeFromCodeCoverage]
public class ValidateModelAttribute : Attribute, IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values
                .SelectMany(modelState => modelState.Errors)
                .Select(modelError => modelError.ErrorMessage);

            context.Result = new BadRequestObjectResult(Result.ErrorList(HttpStatusCode.BadRequest, errors.ToList()));
        }

        await next();
    }
}
