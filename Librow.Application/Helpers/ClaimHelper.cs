using Librow.Application.Common.Enums;
using Librow.Application.Common.Security.Token;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Helpers;

public static class ClaimHelper
{
    public static T? ExtractClaimValue<T>(this IEnumerable<Claim> claims, string claimType, Func<string, T> parser)
    {
        var value = claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (value == null) return default;

        try
        {
            return parser(value);
        }
        catch
        {
            return default;
        }
    }

    public static T? GetItem<T>(HttpContext? context, string key)
    {
        if (context?.Items[key] is T value)
        {
            return value;
        }
        return default;
    }

}


