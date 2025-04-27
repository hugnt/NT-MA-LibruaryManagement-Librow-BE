using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Security.Token;
public class TokenModel
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string JwtId { get; set; }

    public TokenModel(string jwtId, string accessToken, string refreshToken)
    {
        JwtId = jwtId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}

public class TokenValidationModel<T>
{
    public bool IsSuccess { get; set; }
    public T? AttachData { get; set; }
    public string ErrorMessage { get; set; } = "";

    public static TokenValidationModel<T> Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    public static TokenValidationModel<T> Success(T attachData) => new() { IsSuccess = true, AttachData = attachData };
    public static TokenValidationModel<T> Success() => new() { IsSuccess = true };
}

