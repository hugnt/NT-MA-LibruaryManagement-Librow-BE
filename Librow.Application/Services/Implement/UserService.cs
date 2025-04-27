using FluentValidation;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Mappings;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using LoginRequest = Librow.Application.Models.Requests.LoginRequest;
using RegisterRequest = Librow.Application.Models.Requests.RegisterRequest;

namespace Librow.Application.Services.Implement;
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public UserService(IRepository<User> userRepository, IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator, IRepository<RefreshToken> refreshTokenRepository)
    {
        _userRepository = userRepository;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenRepository = refreshTokenRepository;
    }
    public async Task<Result> Login(LoginRequest loginRequest)
    {
        var validateResult = _loginValidator.Validate(loginRequest);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        var selectedAccount = await _userRepository.FirstOrDefaultAsync(x => x.Username == loginRequest.Username);
        if (selectedAccount == null)
        {
            return Result.Error(HttpStatusCode.NotFound ,ErrorMessage.ObjectNotFound(loginRequest.Username,"User"));
        }
        if (!loginRequest.Password.IsValidWith(selectedAccount.PasswordHash))
        {
            return Result.Error(HttpStatusCode.Unauthorized, AccountMessage.PasswordNotCorrect);
        }

        var claims = GetClaims(selectedAccount);
        var token = TokenProvider.GenerateTokens(claims);
        _refreshTokenRepository.Add(new RefreshToken()
        {
            JwtId = token.JwtId,
            UserId = selectedAccount.Id,
            Token = token.RefreshToken,
            IsUsed = false,
            IsRevoked = false,
            IssuedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.AddMinutes(TokenProvider.RefreshTokenExpirationInMinutes)
        });
        await _refreshTokenRepository.SaveChangesAsync();

        var loginResponse = new LoginResponse()
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            User = selectedAccount.ToResponse()
        };

        return Result<LoginResponse>.SuccessWithBody(loginResponse);
    }
    public async Task<Result> Register(RegisterRequest registerRequest)
    {
        var validateResult = _registerValidator.Validate(registerRequest);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        if(await _userRepository.AnyAsync(x => x.Username == registerRequest.Username.Trim().ToLower()))
        {
            return Result.Error(HttpStatusCode.BadRequest, ErrorMessage.ObjectExisted(registerRequest.Username, "User"));
        }

        var userEntity = registerRequest.ToEntity();
        userEntity.PasswordHash = registerRequest.Password.Hash();

         _userRepository.Add(userEntity);
        await _userRepository.SaveChangesAsync();

        var claims = GetClaims(userEntity);
        var token = TokenProvider.GenerateTokens(claims);

        var loginResponse = new LoginResponse()
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            User = userEntity.ToResponse()
        };

        return Result<LoginResponse>.SuccessWithBody(loginResponse);
    }

    public async Task<Result> ExtendSession(ExtendSessionRequest reLoginRequest)
    {
        var validateAccessToken = TokenProvider.ValidateAccessToken(reLoginRequest.AccessToken);
        if (!validateAccessToken.IsSuccess)
        {
            return Result.Error(HttpStatusCode.Unauthorized, validateAccessToken.ErrorMessage);
        }

        var jwtId = validateAccessToken.AttachData!.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)!;
        var validateRefreshToken = await ValidateRefreshToken(new TokenModel(jwtId.Value, reLoginRequest.AccessToken, reLoginRequest.RefreshToken));
        if (!validateRefreshToken.IsSuccess)
        {
            return Result.Error(HttpStatusCode.Unauthorized, validateAccessToken.ErrorMessage);
        }

        //Update status of token 
        var selectedRefreshToken = validateRefreshToken.AttachData!;
        selectedRefreshToken.IsRevoked = true;
        selectedRefreshToken.IsUsed = true;
        _refreshTokenRepository.Update(selectedRefreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        //Create new token
        var userName = validateAccessToken.AttachData!.Claims.FirstOrDefault(x => x.Type == ClaimType.Username)!;
        var selectedUser = await _userRepository.FirstOrDefaultAsync(x => x.Username == userName.Value);
        if(selectedUser == null) return Result.Error(HttpStatusCode.Unauthorized, ErrorMessage.ObjectNotFound(userName,"User"));
        var claims = GetClaims(selectedUser);
        var token = TokenProvider.GenerateTokens(claims);

        var loginResponse = new LoginResponse()
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            User = selectedUser.ToResponse()
        };
        return Result<LoginResponse>.SuccessWithBody(loginResponse);

    }

    private Claim[] GetClaims(User user)
    {
        return [
            new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()),
            new Claim(ClaimType.Username, user.Username),
            new Claim(ClaimType.Id , user.Id.ToString()),
            new Claim(ClaimType.Role , user.Role.ToString())
        ];
    }

    private async Task<TokenValidationModel<RefreshToken>> ValidateRefreshToken(TokenModel tokenModel)
    {
        if (string.IsNullOrEmpty(tokenModel.RefreshToken))
        {
            return TokenValidationModel<RefreshToken>.Error("Refresh Token must not be empty!");
        }
        var selectedRefreshToken = await _refreshTokenRepository.FirstOrDefaultAsync(x=> x.Token == tokenModel.RefreshToken);
        if (selectedRefreshToken == null)
        {
            return TokenValidationModel<RefreshToken>.Error("Refesh token does not exist");
        }
        if (selectedRefreshToken.ExpireAt < DateTime.UtcNow)
        {
            return TokenValidationModel<RefreshToken>.Error("Refesh token has been revoked");
        }
        if (selectedRefreshToken.IsUsed || selectedRefreshToken.IsRevoked)
        {
            return TokenValidationModel<RefreshToken>.Error("Refesh token has been used");
        }
        if (selectedRefreshToken.JwtId != tokenModel.JwtId)
        {
            return TokenValidationModel<RefreshToken>.Error("Jwt Id does not match");
        }
        return TokenValidationModel<RefreshToken>.Success(selectedRefreshToken);
        
    }
}
