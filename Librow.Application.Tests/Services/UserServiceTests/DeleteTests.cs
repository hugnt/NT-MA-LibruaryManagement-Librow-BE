using FluentAssertions;
using FluentValidation;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models.Requests;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.UserServiceTests
{
    public class DeleteTests
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

        public DeleteTests()
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
        public async Task Delete_ExistingUser_ReturnsSuccessNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updatedBy = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
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
            _userRepositoryMock.Setup(r => r.Update(It.IsAny<User>()));
            _userRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.Delete(userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _userRepositoryMock.Verify(r => r.Update(It.Is<User>(u =>
                u.Id == userId &&
                u.IsDeleted == true &&
                u.UpdatedBy == updatedBy &&
                u.UpdatedAt != default)), Times.Once());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Delete_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Id = userId, IsDeleted = false })),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.Delete(userId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never());
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
