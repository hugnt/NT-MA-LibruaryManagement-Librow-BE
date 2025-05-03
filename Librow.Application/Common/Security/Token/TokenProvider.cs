using Librow.Application.Common.Enums;
using Librow.Application.Common.Messages;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Librow.Application.Common.Security.Token;
public static class TokenProvider
{
    private static int _accessTokenExprirationInMinutes = 1;
    private static string _secretKey = "5d2cb8e11157b7dbc8f7a4cb28734f4e7b265863ea9a7bfda9e58f12078e7c58";
    public static int RefreshTokenExpirationInMinutes = 2;
    public static TokenValidationParameters TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
        ClockSkew = TimeSpan.Zero,
        ValidateLifetime = true
    };
    public static void SetSecretKey(string? value)
    {
        if (value != null)
        {
            _secretKey = value;
            TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        }
    }
    public static void SetAccessTokenExprirationTime(int value)
    {
        if (value >= 0)
        {
            _accessTokenExprirationInMinutes = value;
        }
    }
    public static void SetRefreshTokenExprirationTime(int value)
    {
        if (value >= 0)
        {
            RefreshTokenExpirationInMinutes = value;
        }
    }
    public static TokenModel GenerateTokens(Claim[] claims)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var secretKeyBytes = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExprirationInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes),
                                                         SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescription);
        var accsessToken = jwtTokenHandler.WriteToken(token);
        var refeshToken = GenerateRefeshToken();
        return new TokenModel(token.Id, accsessToken, refeshToken);
    }
    public static TokenValidationModel<ClaimsPrincipal> ValidateAccessToken(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return TokenValidationModel<ClaimsPrincipal>.Error("Token must not be empty!");
        }
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = jwtTokenHandler.ValidateToken(accessToken, TokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return TokenValidationModel<ClaimsPrincipal>.Error(TokenMessage.InvalidTokenType);
            }

            if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                return TokenValidationModel<ClaimsPrincipal>.Error(string.Format(TokenMessage.InvalidAlgorithm, jwtToken.Header.Alg));
            }

            return TokenValidationModel<ClaimsPrincipal>.Success(principal);
        }
        catch (SecurityTokenExpiredException)
        {
            return TokenValidationModel<ClaimsPrincipal>.ErrorWithCode(TokenErrorCode.TokenExpired,TokenMessage.TokenExpired);
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            return TokenValidationModel<ClaimsPrincipal>.ErrorWithCode(TokenErrorCode.TokenSignatureKeyNotFound, TokenMessage.InvalidSignatureKey);
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return TokenValidationModel<ClaimsPrincipal>.ErrorWithCode(TokenErrorCode.TokenInvalidSignature, TokenMessage.InvalidSignature);
        }
        catch (SecurityTokenException ex)
        {
            return TokenValidationModel<ClaimsPrincipal>.Error(string.Format(TokenMessage.TokenValidationFailed, ex.Message));
        }
        catch (Exception)
        {
            return TokenValidationModel<ClaimsPrincipal>.Error(TokenMessage.UnexpectedError);
        }
    }

    public static ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var identity = new ClaimsIdentity(jwtToken.Claims);
        return new ClaimsPrincipal(identity);
    }


    private static string GenerateRefeshToken()
    {
        var random = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
            return Convert.ToBase64String(random);
        }
    }


}

