using Librow.Application.Common.Enums;
using Librow.Application.Common.Security.Token;
using Librow.Application.Helpers;

namespace Librow.API.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITokenService tokenService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var validateAccessToken = tokenService.ValidateAccessToken(token);
        if (validateAccessToken.IsSuccess && validateAccessToken.AttachData?.Claims != null)
        {
            var claims = validateAccessToken.AttachData.Claims;

            context.Items[ClaimType.Role] = claims.ExtractClaimValue(ClaimType.Role, Enum.Parse<AuthRole>);
            context.Items[ClaimType.Id] = claims.ExtractClaimValue(ClaimType.Id, Guid.Parse);
        }
        else
        {
            context.Items[ClaimType.Role] = null;
            context.Items[ClaimType.Id] = null;
        }
        await _next(context);
    }
}