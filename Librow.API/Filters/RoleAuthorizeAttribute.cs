using Librow.Application.Common.Enums;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Net;


namespace Librow.API.Filters;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly AuthRole _requiredRole;

    public RoleAuthorizeAttribute(AuthRole roleSet = AuthRole.Customer)
    {
        _requiredRole = roleSet;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            return;
        }

        if (context.HttpContext.Items[ClaimType.Role] is not AuthRole roleReceived)
        {
            context.Result = new JsonResult(Result.Error(HttpStatusCode.Unauthorized, "Token invalid or missing"))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        if (roleReceived != AuthRole.Admin && roleReceived != _requiredRole)
        {
            context.Result = new JsonResult(Result.Error(HttpStatusCode.Forbidden, "You do not have permission to perform this action"))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }
    }
}