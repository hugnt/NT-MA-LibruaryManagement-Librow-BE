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

    public static T? GetClaimValue<T>(HttpContext? context, string claimType)
    {
        var value = context?.User?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (string.IsNullOrEmpty(value)) return default;

        try
        {
            var targetType = typeof(T);
            if (targetType.IsEnum && Enum.TryParse(targetType, value, true, out var enumValue))
            {
                return (T?)enumValue;
            }
            // Guid
            if (targetType == typeof(Guid) && Guid.TryParse(value, out var guidResult))
            {
                return (T)(object)guidResult;
            }
            return (T)Convert.ChangeType(value, targetType);
        }
        catch
        {
            
            return default;
        }
    }

}


