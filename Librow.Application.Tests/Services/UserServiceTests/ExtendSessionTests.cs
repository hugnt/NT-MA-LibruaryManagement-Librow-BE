using FluentValidation;
using Librow.Application.Common.Enums;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Models;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq.Expressions;
using System.IdentityModel.Tokens.Jwt;

namespace Librow.Application.Tests.Services.UserServiceTests
{
    public class ExtendSessionTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepositoryMock;
        private readonly Mock<IValidator<RegisterRequest>> _registerValidatorMock;
        private readonly Mock<IRepository<AuditLog>> _auditLogRepositoryMock;
        private readonly Mock<IValidator<UserUpdateRequest>> _userUpdateValidatorMock;
        private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserService _userService;

        public ExtendSessionTests()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _tokenServiceMock = new Mock<ITokenService>();
            _refreshTokenRepositoryMock = new Mock<IRepository<RefreshToken>>();
            _registerValidatorMock = new Mock<IValidator<RegisterRequest>>();
            _auditLogRepositoryMock = new Mock<IRepository<AuditLog>>();
            _userUpdateValidatorMock = new Mock<IValidator<UserUpdateRequest>>();
            _loginValidatorMock = new Mock<IValidator<LoginRequest>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _registerValidatorMock.Object,
                _loginValidatorMock.Object,
                _refreshTokenRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _userUpdateValidatorMock.Object,
                _auditLogRepositoryMock.Object,
                _tokenServiceMock.Object);
        }

        [Fact]
        public async Task ExtendSession_MissingAccessToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new ExtendSessionRequest { RefreshToken = "refresh-token" };
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            // Act
            var result = await _userService.ExtendSession(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _tokenServiceMock.Verify(t => t.ValidateAccessToken(It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task ExtendSession_ValidAccessToken_ReturnsExistingTokens()
        {
            // Arrange
            var request = new ExtendSessionRequest { RefreshToken = "refresh-token" };
            var accessToken = "valid-access-token";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            _tokenServiceMock.Setup(t => t.ValidateAccessToken(accessToken))
                .Returns(TokenValidationModel<ClaimsPrincipal>.Success(new ClaimsPrincipal()));

            // Act
            var result = await _userService.ExtendSession(request);
            var tokenResult = result.As<Result<TokenResponse>>();

            // Assert
            tokenResult.IsSuccess.Should().BeTrue();
            tokenResult.StatusCode.Should().Be(HttpStatusCode.OK);
            tokenResult.Data.AccessToken.Should().Be(accessToken);
            tokenResult.Data.RefreshToken.Should().Be(request.RefreshToken);
            _refreshTokenRepositoryMock.Verify(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<RefreshToken, object>>[]>()), Times.Never());
        }

        [Fact]
        public async Task ExtendSession_ExpiredAccessToken_InvalidRefreshToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new ExtendSessionRequest { RefreshToken = "invalid-refresh-token" };
            var accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIxYTU5YjRkMS0xMzIxLTQxNjYtYjYxNC1iMDRlODhjZmVkZTQiLCJVc2VybmFtZSI6ImFkbWluIiwiSWQiOiIzZjhiMmExZS01YzRkLTRlOWYtYTJiMy03YzhkOWUwZjFhMmIiLCJSb2xlIjoiQWRtaW4iLCJuYmYiOjE3NDY1MTU5MjgsImV4cCI6MTc0NjUxNTk4OCwiaWF0IjoxNzQ2NTE1OTI4fQ.bdvOr0dTQSmPVMLdDwyvk11Y9gJ2cVCfPoxAH2AE-48";
            var jwtId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            _tokenServiceMock.Setup(t => t.ValidateAccessToken(accessToken))
                .Returns(TokenValidationModel<ClaimsPrincipal>.ErrorWithCode(TokenErrorCode.TokenExpired, "Token expired"));
            _refreshTokenRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<RefreshToken, bool>>>(expr => expr.Compile()(new RefreshToken { Token = request.RefreshToken })),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<RefreshToken, object>>[]>()))
                .ReturnsAsync((RefreshToken)null);

            // Act
            var result = await _userService.ExtendSession(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _refreshTokenRepositoryMock.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Never());
            _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Never());
        }

        [Fact]
        public async Task ExtendSession_ExpiredAccessToken_ValidRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var request = new ExtendSessionRequest { RefreshToken = "valid-refresh-token" };
            var accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIxYTU5YjRkMS0xMzIxLTQxNjYtYjYxNC1iMDRlODhjZmVkZTQiLCJVc2VybmFtZSI6ImFkbWluIiwiSWQiOiIzZjhiMmExZS01YzRkLTRlOWYtYTJiMy03YzhkOWUwZjFhMmIiLCJSb2xlIjoiQWRtaW4iLCJuYmYiOjE3NDY1MTU5MjgsImV4cCI6MTc0NjUxNTk4OCwiaWF0IjoxNzQ2NTE1OTI4fQ.bdvOr0dTQSmPVMLdDwyvk11Y9gJ2cVCfPoxAH2AE-48";
            var jwtId = "1a59b4d1-1321-4166-b614-b04e88cfede4";
            var userId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

  
            _tokenServiceMock.Setup(t => t.ValidateAccessToken(accessToken))
                .Returns(TokenValidationModel<ClaimsPrincipal>.ErrorWithCode(TokenErrorCode.TokenExpired, "Token expired"));
            var refreshToken = new RefreshToken
            {
                JwtId = jwtId,
                UserId = userId,
                Token = request.RefreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow.AddMinutes(-10),
                ExpireAt = DateTime.UtcNow.AddMinutes(10)
            };
            _refreshTokenRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<RefreshToken, bool>>>(expr => expr.Compile()(new RefreshToken { Token = request.RefreshToken })),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<RefreshToken, object>>[]>()))
                .ReturnsAsync(refreshToken);
            _refreshTokenRepositoryMock.Setup(r => r.Update(It.IsAny<RefreshToken>()));
            _refreshTokenRepositoryMock.Setup(r => r.Add(It.IsAny<RefreshToken>()));
            _refreshTokenRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var newToken = new TokenModel(jwtId, "new-access-token", "new-refresh-token");
            _tokenServiceMock.Setup(t => t.GenerateTokens(It.IsAny<Claim[]>()))
                .Returns(newToken);

            // Act
            var result = await _userService.ExtendSession(request);
            var tokenResult = result.As<Result<TokenResponse>>();

            // Assert
            tokenResult.IsSuccess.Should().BeTrue();
            tokenResult.StatusCode.Should().Be(HttpStatusCode.OK);
            tokenResult.Data.AccessToken.Should().Be(newToken.AccessToken);
            tokenResult.Data.RefreshToken.Should().Be(newToken.RefreshToken);
            _refreshTokenRepositoryMock.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Once());
            _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once());
            _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

    }
}