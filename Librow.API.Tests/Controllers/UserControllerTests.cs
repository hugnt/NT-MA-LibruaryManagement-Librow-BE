using FluentAssertions;
using Librow.API.Controllers;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Librow.API.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOkResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "testuser", Password = "testpassword" };
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _userServiceMock.Setup(service => service.Login(loginRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Register_ShouldReturnCreatedResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequest { Username = "newuser", Password = "newpassword" };
        var expectedResult = Result.Success(HttpStatusCode.Created, "User registered successfully");

        _userServiceMock.Setup(service => service.Register(registerRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var createdResult = result as ObjectResult;
        createdResult.Should().NotBeNull();
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Logout_ShouldReturnOkResponse_WhenUserIsLoggedIn()
    {
        // Arrange
        var logoutRequest = new LogoutRequest { RefreshToken = "some_refresh_token" };
        var expectedResult = Result.Success(HttpStatusCode.OK, "User logged out successfully");

        _userServiceMock.Setup(service => service.Logout(logoutRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Logout(logoutRequest);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExtendSession_ShouldReturnOkResponse()
    {
        // Arrange
        var extendSessionRequest = new ExtendSessionRequest { RefreshToken = "some_refresh_token" };
        var expectedResult = Result.Success(HttpStatusCode.OK, "Session extended successfully");

        _userServiceMock.Setup(service => service.ExtendSession(extendSessionRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ExtendSession(extendSessionRequest);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetCurrentUserContext_ShouldReturnOkResponse()
    {
        // Arrange
        var expectedResult = Result<object>.SuccessWithBody(new object());

        _userServiceMock.Setup(service => service.GetCurrentUserContext())
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetCurrentUserContext();

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResponse_WhenAdmin()
    {
        // Arrange
        var filter = new FilterRequest();
        var expectedResult = Result<object>.SuccessWithBody(new object());

        _userServiceMock.Setup(service => service.GetAll(filter))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkResponse_WhenAdmin()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result<object>.SuccessWithBody(new object());

        _userServiceMock.Setup(service => service.GetById(id))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Add_ShouldReturnCreatedResponse_WhenAdmin()
    {
        // Arrange
        var registerRequest = new RegisterRequest { Username = "newadmin", Password = "newpassword" };
        var expectedResult = Result.Success(HttpStatusCode.Created, "User added successfully");

        _userServiceMock.Setup(service => service.Add(registerRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Add(registerRequest);

        // Assert
        var createdResult = result as ObjectResult;
        createdResult.Should().NotBeNull();
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Update_ShouldReturnOkResponse_WhenAdmin()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updatedUser = new UserUpdateRequest { Username = "updatedUser" };
        var expectedResult = Result<object>.SuccessWithBody(new object());

        _userServiceMock.Setup(service => service.Update(id, updatedUser))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(id, updatedUser);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Delete_ShouldReturnOkResponse_WhenAdmin()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result.Success(HttpStatusCode.OK, "User deleted successfully");

        _userServiceMock.Setup(service => service.Delete(id))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetActivityLog_ShouldReturnOkResponse_WhenAdmin()
    {
        // Arrange
        var filter = new FilterRequest();
        var expectedResult = Result<object>.SuccessWithBody(new object());

        _userServiceMock.Setup(service => service.GetActivityLog(filter))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetActivityLog(filter);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }
}
