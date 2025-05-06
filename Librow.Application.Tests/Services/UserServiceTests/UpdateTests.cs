using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models.Requests;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Tests.Services.UserServiceTests
{
    public class UpdateTests
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

        public UpdateTests()
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
        public async Task Update_ValidRequest_ReturnsSuccessNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updatedBy = Guid.NewGuid();
            var updateRequest = new UserUpdateRequest
            {
                Email = "updated@example.com",
                Username = "updateduser"
            };
            var user = new User
            {
                Id = userId,
                Username = "olduser",
                Email = "old@example.com",
                Role = Role.Customer,
                IsDeleted = false
            };

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("Id", updatedBy.ToString()) }));
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Id = userId, IsDeleted = false })),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Email = "updated@example.com" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userUpdateValidatorMock.Setup(v => v.Validate(updateRequest))
                .Returns(new ValidationResult());
            _userRepositoryMock.Setup(r => r.Update(It.IsAny<User>()));
            _userRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.Update(userId, updateRequest);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _userRepositoryMock.Verify(r => r.Update(It.Is<User>(u =>
                u.Id == userId &&
                u.Email == updateRequest.Email.Trim().ToLower() &&
                u.Username == updateRequest.Username &&
                u.UpdatedBy == updatedBy &&
                u.UpdatedAt != default)), Times.Once());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Update_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateRequest = new UserUpdateRequest
            {
                Email = "updated@example.com",
                Username = "updateduser"
            };

            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Id = userId, IsDeleted = false })),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.Update(userId, updateRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _userRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());
            _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task Update_EmailExists_ReturnsBadRequestError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateRequest = new UserUpdateRequest
            {
                Email = "existing@example.com",
                Username = "updateduser"
            };
            var user = new User
            {
                Id = userId,
                Username = "olduser",
                Email = "old@example.com",
                Role = Role.Customer,
                IsDeleted = false
            };

            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Id = userId, IsDeleted = false })),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Email = "existing@example.com" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.Update(userId, updateRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task Update_InvalidValidation_ReturnsErrorValidation()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateRequest = new UserUpdateRequest
            {
                Email = "",
                Username = ""
            };
            var user = new User
            {
                Id = userId,
                Username = "olduser",
                Email = "old@example.com",
                Role = Role.Customer,
                IsDeleted = false
            };
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Email is required"),
                new ValidationFailure("Username", "Username is required")
            });

            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Id = userId, IsDeleted = false })),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.AnyAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Email = "" })),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _userUpdateValidatorMock.Setup(v => v.Validate(updateRequest))
                .Returns(validationResult);

            // Act
            var result = await _userService.Update(userId, updateRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Errors.Should().Contain("Email is required");
            result.Errors.Should().Contain("Username is required");
            _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
