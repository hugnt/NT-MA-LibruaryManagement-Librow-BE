using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Librow.Application.Tests.Services.UserServiceTests
{
    public class LoginTests
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

        public LoginTests()
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
        public async Task Login_ValidCredentials_ReturnsSuccessWithLoginResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "123456" };
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                PasswordHash = "C40D0CF1F0815D27829F76BA3F7B0399A9FF5BD6C05252B7F500B6826419EE25-E41A6B82F54C202A240A483B224F15C3",
                Role = Role.Customer
            };
            var token = new TokenModel("jwt_id", "access_token", "refresh_token");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Username", user.Username),
                new Claim("Id", user.Id.ToString()),
                new Claim("Role", user.Role.ToString())
            };

            _loginValidatorMock.Setup(v => v.Validate(loginRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateTokens(It.IsAny<Claim[]>()))
                .Returns(token);
            _refreshTokenRepositoryMock.Setup(r => r.Add(It.IsAny<RefreshToken>()))
                .Verifiable();
            _refreshTokenRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.Login(loginRequest);
            var loginResult = result.As<Result<LoginResponse>>();

            // Assert
            loginResult.IsSuccess.Should().BeTrue();
            loginResult.StatusCode.Should().Be(HttpStatusCode.OK);
            loginResult.Data.Should().NotBeNull();
            loginResult.Data.AccessToken.Should().Be(token.AccessToken);
            loginResult.Data.RefreshToken.Should().Be(token.RefreshToken);
            loginResult.Data.User.Username.Should().Be(user.Username);
            _refreshTokenRepositoryMock.Verify(r => r.Add(It.Is<RefreshToken>(rt =>
                rt.JwtId == token.JwtId &&
                rt.UserId == user.Id &&
                rt.Token == token.RefreshToken &&
                rt.IsUsed == false &&
                rt.IsRevoked == false)), Times.Once());
            _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Login_InvalidValidation_ReturnsErrorValidation()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "", Password = "" };
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Username", "Username is required"),
                new ValidationFailure("Password", "Password is required")
            });

            _loginValidatorMock.Setup(v => v.Validate(loginRequest))
                .Returns(validationResult);

            // Act
            var result = await _userService.Login(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Errors.Should().Contain("Username is required");
            result.Errors.Should().Contain("Password is required");
            _userRepositoryMock.Verify(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()), Times.Never());
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "nonexistent", Password = "password" };

            _loginValidatorMock.Setup(v => v.Validate(loginRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.Login(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _tokenServiceMock.Verify(t => t.GenerateTokens(It.IsAny<Claim[]>()), Times.Never());
        }

        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorizedError()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "wrongpassword" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "C40D0CF1F0815D27829F76BA3F7B0399A9FF5BD6C05252B7F500B6826419EE25-E41A6B82F54C202A240A483B224F15C3",
                Role = Role.Customer
            };

            _loginValidatorMock.Setup(v => v.Validate(loginRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.Login(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _tokenServiceMock.Verify(t => t.GenerateTokens(It.IsAny<Claim[]>()), Times.Never());
        }
    }
}