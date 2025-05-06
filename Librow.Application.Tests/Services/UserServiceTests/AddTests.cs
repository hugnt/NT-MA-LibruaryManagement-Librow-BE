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
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.UserServiceTests
{
    public class AddTests
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

        public AddTests()
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
        public async Task Add_ValidRequest_ReturnsSuccessWithLoginResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password",
                Role = Role.Customer
            };
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "newuser",
                Email = "newuser@example.com",
                PasswordHash = "hashedpassword",
                Role = Role.Customer,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            var token = new TokenModel("jwt_id", "access_token", "refresh_token");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Username", user.Username),
                new Claim("Id", user.Id.ToString()),
                new Claim("Role", user.Role.ToString())
            };

            _registerValidatorMock.Setup(v => v.Validate(registerRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Username = "newuser" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Email = "newuser@example.com" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(r => r.Add(It.IsAny<User>()))
                .Callback<User>(u => u.Id = userId);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(t => t.GenerateTokens(It.IsAny<Claim[]>()))
                .Returns(token);

            // Act
            var result = await _userService.Add(registerRequest);
            var loginResult = result.As<Result<LoginResponse>>();

            // Assert
            loginResult.IsSuccess.Should().BeTrue();
            loginResult.StatusCode.Should().Be(HttpStatusCode.OK);
            loginResult.Data.Should().NotBeNull();
            loginResult.Data.AccessToken.Should().Be(token.AccessToken);
            loginResult.Data.RefreshToken.Should().Be(token.RefreshToken);
            loginResult.Data.User.Username.Should().Be(user.Username);
            _userRepositoryMock.Verify(r => r.Add(It.Is<User>(u =>
                u.Username == registerRequest.Username.Trim().ToLower() &&
                u.Email == registerRequest.Email.Trim().ToLower() &&
                u.Role == registerRequest.Role)), Times.Once());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Add_InvalidValidation_ReturnsErrorValidation()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "",
                Email = "",
                Password = ""
            };
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Username", "Username is required"),
                new ValidationFailure("Email", "Email is required"),
                new ValidationFailure("Password", "Password is required")
            });

            _registerValidatorMock.Setup(v => v.Validate(registerRequest))
                .Returns(validationResult);

            // Act
            var result = await _userService.Add(registerRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Errors.Should().Contain("Username is required");
            result.Errors.Should().Contain("Email is required");
            result.Errors.Should().Contain("Password is required");
            _userRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());
            _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task Add_UsernameExists_ReturnsBadRequestError()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "existinguser",
                Email = "newuser@example.com",
                Password = "password",
                Role = Role.Customer
            };

            _registerValidatorMock.Setup(v => v.Validate(registerRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Username = "existinguser" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.Add(registerRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _userRepositoryMock.Verify(r => r.AnyAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()), Times.Once());
            _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task Add_EmailExists_ReturnsBadRequestError()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Email = "existing@example.com",
                Password = "password",
                Role = Role.Customer
            };

            _registerValidatorMock.Setup(v => v.Validate(registerRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Username = "newuser" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Email = "existing@example.com" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.Add(registerRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _userRepositoryMock.Verify(r => r.AnyAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
            _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task Add_AdminRole_SetsCreatedByAndUpdatedBy()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newadmin",
                Email = "newadmin@example.com",
                Password = "password",
                Role = Role.Admin
            };
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "newadmin",
                Email = "newadmin@example.com",
                PasswordHash = "hashedpassword",
                Role = Role.Admin,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            var token = new TokenModel("jwt_id", "access_token", "refresh_token");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Username", user.Username),
                new Claim("Id", user.Id.ToString()),
                new Claim("Role", user.Role.ToString())
            };

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("Id", adminId.ToString()) }));
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            _registerValidatorMock.Setup(v => v.Validate(registerRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Username = "newadmin" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Email = "newadmin@example.com" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(r => r.Add(It.IsAny<User>()))
                .Callback<User>(u => u.Id = userId);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(t => t.GenerateTokens(It.IsAny<Claim[]>()))
                .Returns(token);

            // Act
            var result = await _userService.Add(registerRequest);
            var loginResult = result.As<Result<LoginResponse>>();

            // Assert
            loginResult.IsSuccess.Should().BeTrue();
            loginResult.StatusCode.Should().Be(HttpStatusCode.OK);
            _userRepositoryMock.Verify(r => r.Add(It.Is<User>(u =>
                u.Username == registerRequest.Username.Trim().ToLower() &&
                u.Email == registerRequest.Email.Trim().ToLower() &&
                u.Role == registerRequest.Role &&
                u.CreatedBy == adminId &&
                u.UpdatedBy == adminId)), Times.Once());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}