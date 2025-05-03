using Azure.Core;
using FluentValidation;
using Librow.Application.Common.Enums;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security;
using Librow.Application.Common.Security.Token;
using Librow.Application.Helpers;
using Librow.Application.Models;
using Librow.Application.Models.Mappings;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Validators;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
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
    private readonly IValidator<UserUpdateRequest> _userUpdateValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IRepository<User> userRepository, IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator, IRepository<RefreshToken> refreshTokenRepository, IHttpContextAccessor httpContextAccessor, IValidator<UserUpdateRequest> userUpdateValidator)
    {
        _userRepository = userRepository;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenRepository = refreshTokenRepository;
        _httpContextAccessor = httpContextAccessor;
        _userUpdateValidator = userUpdateValidator;
    }


    //LOGIN
    public async Task<Result> Login(LoginRequest loginRequest)
    {
        var validateResult = _loginValidator.Validate(loginRequest);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        var selectedAccount = await _userRepository.FirstOrDefaultAsync(x => x.Username == loginRequest.Username && !x.IsDeleted);
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
        registerRequest.Role = Role.Customer;
        return await Add(registerRequest);
    }
    public async Task<Result> Logout(LogoutRequest logoutRequest)
    {
        var accessToken = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var claimsFromToken = TokenProvider.GetPrincipalFromToken(accessToken!).Claims;
        var jwtId = claimsFromToken.ExtractClaimValue(JwtRegisteredClaimNames.Jti, Convert.ToString)!;
        var validateRefreshToken = await ValidateRefreshToken(new TokenModel(jwtId, accessToken, logoutRequest.RefreshToken));
        if (!validateRefreshToken.IsSuccess)
        {
            return Result.Error(HttpStatusCode.Unauthorized, validateRefreshToken.ErrorMessage);
        }

        //Update status of token 
        var selectedRefreshToken = validateRefreshToken.AttachData!;
        selectedRefreshToken.IsRevoked = true;
        selectedRefreshToken.IsUsed = true;
        _refreshTokenRepository.Update(selectedRefreshToken);
        await _refreshTokenRepository.SaveChangesAsync();
        return Result.SuccessNoContent();
    }
    public async Task<Result> ExtendSession(ExtendSessionRequest extendSessionRequest)
    {
        var accessToken = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if(accessToken == null)
        {
            return Result.Error(HttpStatusCode.Unauthorized, ErrorMessage.UserHasNoPermission);
        }
        var validateAccessToken = TokenProvider.ValidateAccessToken(accessToken);
        if (validateAccessToken.IsSuccess)
        {
            return Result<TokenResponse>.SuccessWithBody(new()
            {
                AccessToken = accessToken,
                RefreshToken = extendSessionRequest.RefreshToken,
            });
        }
        if(validateAccessToken.ErrorCode != TokenErrorCode.TokenExpired)
        {
            return Result.Error(HttpStatusCode.Unauthorized, validateAccessToken.ErrorMessage);
        }
        var claimsFromToken = TokenProvider.GetPrincipalFromToken(accessToken).Claims;
        var jwtId = claimsFromToken.ExtractClaimValue(JwtRegisteredClaimNames.Jti, Convert.ToString)!;
        var validateRefreshToken = await ValidateRefreshToken(new TokenModel(jwtId, accessToken, extendSessionRequest.RefreshToken));
        if (!validateRefreshToken.IsSuccess)
        {
            return Result.Error(HttpStatusCode.Unauthorized, validateRefreshToken.ErrorMessage);
        }

        //Update status of token 
        var selectedRefreshToken = validateRefreshToken.AttachData!;
        selectedRefreshToken.IsRevoked = true;
        selectedRefreshToken.IsUsed = true;
        _refreshTokenRepository.Update(selectedRefreshToken);

        //Create new token
        var userId = claimsFromToken.ExtractClaimValue(ClaimType.Id, Guid.Parse)!;
        var userName = claimsFromToken.ExtractClaimValue(ClaimType.Username, Convert.ToString)!;
        var role = claimsFromToken.ExtractClaimValue(ClaimType.Role, Enum.Parse<Role>)!;
        var userClaims = new User()
        {
            Id = userId,
            Username = userName!,
            Role = role
        };
        var claims = GetClaims(userClaims);
        var token = TokenProvider.GenerateTokens(claims);

        //add new refesh token
        _refreshTokenRepository.Add(new RefreshToken()
        {
            JwtId = token.JwtId,
            UserId = userId,
            Token = token.RefreshToken,
            IsUsed = false,
            IsRevoked = false,
            IssuedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.AddMinutes(TokenProvider.RefreshTokenExpirationInMinutes)
        });
        await _refreshTokenRepository.SaveChangesAsync();

        var tokenResponse = new TokenResponse()
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
        };
        return Result<TokenResponse>.SuccessWithBody(tokenResponse);

    }
    public async Task<Result> GetCurrentUserContext()
    {
        var currentUserId = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
        var selectedUserEntity = await _userRepository.FirstOrDefaultAsync(x=>x.Id ==  currentUserId);
        if (selectedUserEntity == null)
        {
            return Result.Error(HttpStatusCode.Unauthorized, ErrorMessage.ObjectNotFound(currentUserId, "User"));
        }
        return Result<UserResponse>.SuccessWithBody(selectedUserEntity.ToResponse());
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

    //CRUD
    public async Task<Result> GetAll(FilterRequest filter)
    {
        var res = await _userRepository.GetByFilterAsync(
            pageSize: filter.PageSize,
            pageNumber: filter.PageNumber,
            predicate: x => !x.IsDeleted,
            selectQuery: UserMapping.SelectResponseExpression
        );
        return FilterResult<List<UserResponse>>.Success(res.Data.ToList(), res.TotalCount);
    }

    public async Task<Result> GetById(Guid id)
    {
        var selectedEntity = await _userRepository.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, selectQuery: UserMapping.SelectResponseExpression);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Book "));
        }
        return Result<UserResponse>.SuccessWithBody(selectedEntity);
    }

    public async Task<Result> Add(RegisterRequest registerRequest)
    {
        var validateResult = _registerValidator.Validate(registerRequest);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        if (await _userRepository.AnyAsync(x => x.Username == registerRequest.Username.Trim().ToLower()))
        {
            return Result.Error(HttpStatusCode.BadRequest, ErrorMessage.ObjectExisted(registerRequest.Username, "User"));
        }
        if (await _userRepository.AnyAsync(x => x.Email == registerRequest.Email.Trim().ToLower()))
        {
            return Result.Error(HttpStatusCode.BadRequest, ErrorMessage.ObjectExisted(registerRequest.Email, "User with email"));
        }

        var userEntity = registerRequest.ToEntity();
        userEntity.PasswordHash = registerRequest.Password.Hash();
        userEntity.CreatedAt = userEntity.UpdatedAt = DateTime.Now;
        if (registerRequest.Role == Role.Admin)
        {
            userEntity.CreatedBy = userEntity.UpdatedBy = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
        }
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

    public async Task<Result> Update(Guid id, UserUpdateRequest updatedUser)
    {
        var selectedEntity = await _userRepository.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "User"));
        }
        if (selectedEntity.Email != updatedUser.Email.Trim().ToLower())
        {
            if (await _userRepository.AnyAsync(x => x.Email == updatedUser.Email.Trim().ToLower()))
            {
                return Result.Error(HttpStatusCode.BadRequest, ErrorMessage.ObjectExisted(updatedUser.Email, "User with email"));
            }
        }
        var validateResult = _userUpdateValidator.Validate(updatedUser);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        selectedEntity.MappingFieldFrom(updatedUser);
        selectedEntity.UpdatedAt = DateTime.Now;
        selectedEntity.UpdatedBy = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);

        _userRepository.Update(selectedEntity);
        await _userRepository.SaveChangesAsync();
        return Result.SuccessNoContent();
    }

    public async Task<Result> Delete(Guid id)
    {
        var selectedEntity = await _userRepository.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "User"));
        }
        selectedEntity.UpdatedAt = DateTime.Now;
        selectedEntity.UpdatedBy = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
        selectedEntity.IsDeleted = true;
        _userRepository.Update(selectedEntity);
        await _userRepository.SaveChangesAsync();

        return Result.SuccessNoContent();
    }
}
