using Librow.Application.Common.Email;
using Librow.Application.Common.Enums;
using Librow.Application.Common.Messages;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Librow.Application.Common.Security.Token;

public interface ITokenService
{
    public TokenModel GenerateTokens(Claim[] claims);
    public TokenValidationModel<ClaimsPrincipal> ValidateAccessToken(string? accessToken);
}
public class TokenService : ITokenService
{

    private readonly TokenSettings _tokenSettings;
    public TokenService(IOptions<TokenSettings> tokenSettings)
    {
        _tokenSettings = tokenSettings.Value;
        TokenProvider.RefreshTokenExpirationInMinutes = _tokenSettings.RefreshTokenExpirationInMinutes;
        TokenProvider.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));
    }

    public TokenModel GenerateTokens(Claim[] claims)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var secretKeyBytes = Encoding.UTF8.GetBytes(_tokenSettings.SecretKey);
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_tokenSettings.AccessTokenExpirationInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes),
                                                         SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescription);
        var accsessToken = jwtTokenHandler.WriteToken(token);
        var refeshToken = TokenProvider.GenerateRefeshToken();
        return new TokenModel(token.Id, accsessToken, refeshToken);
    }

    public TokenValidationModel<ClaimsPrincipal> ValidateAccessToken(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return TokenValidationModel<ClaimsPrincipal>.Error("Token must not be empty!");
        }
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = jwtTokenHandler.ValidateToken(accessToken, TokenProvider.TokenValidationParameters, out var validatedToken);

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


}

