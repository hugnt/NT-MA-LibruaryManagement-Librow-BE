using FluentValidation;
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
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FluentAssertions;

namespace Librow.Application.Tests.Services.UserServiceTests
{
    public class GetByIdTests
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

        public GetByIdTests()
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
        public async Task GetById_ExistingUser_ReturnsSuccessWithUserResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                Role = Role.Customer,
                IsDeleted = false
            };
            var userResponse = new UserResponse { Username = user.Username };

            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Id = userId, IsDeleted = false })),
                It.IsAny<Expression<Func<User, UserResponse>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _userService.GetById(userId);
            var userResult = result.As<Result<UserResponse>>();

            // Assert
            userResult.IsSuccess.Should().BeTrue();
            userResult.StatusCode.Should().Be(HttpStatusCode.OK);
            userResult.Data.Should().NotBeNull();
            userResult.Data.Username.Should().Be(userResponse.Username);
        }

        [Fact]
        public async Task GetById_UserNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<Expression<Func<User, bool>>>(expr => expr.Compile()(new User { Id = userId, IsDeleted = false })),
                It.IsAny<Expression<Func<User, UserResponse>>>(),
                 It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()
                )).ReturnsAsync((UserResponse)null);

            // Act
            var result = await _userService.GetById(userId);
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
