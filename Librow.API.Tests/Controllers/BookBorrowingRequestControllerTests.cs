using FluentAssertions;
using Librow.API.Controllers;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using Xunit;

namespace Librow.API.Tests.Controllers;

public class BookBorrowingRequestControllerTests
{
    private readonly Mock<IBookBorrowingRequestService> _bookBorrowingRequestServiceMock;
    private readonly BookBorrowingRequestController _controller;

    public BookBorrowingRequestControllerTests()
    {
        _bookBorrowingRequestServiceMock = new Mock<IBookBorrowingRequestService>();
        _controller = new BookBorrowingRequestController(_bookBorrowingRequestServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResponse()
    {
        // Arrange
        var filter = new BorrowingRequestFilter();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookBorrowingRequestServiceMock.Setup(service => service.GetAll(filter))
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
    public async Task GetUserRequestInfo_ShouldReturnOkResponse()
    {
        // Arrange
        var filter = new RequestFilter();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookBorrowingRequestServiceMock.Setup(service => service.GetUserRequestInfo(filter))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetUserRequestInfo(filter);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetAllBorrowingBooks_ShouldReturnOkResponse()
    {
        // Arrange
        var filter = new BorrowingRequestFilter();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookBorrowingRequestServiceMock.Setup(service => service.GetAllBorrowingBooks(filter))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAllBorrowingBooks(filter);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookBorrowingRequestServiceMock.Setup(service => service.GetById(id))
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
    public async Task Add_ShouldReturnCreatedResponse()
    {
        // Arrange
        var newBookBorrowingRequest = new BorrowingRequest();
        var expectedResult = Result.Success(HttpStatusCode.Created, "Book borrowing request added successfully");

        _bookBorrowingRequestServiceMock.Setup(service => service.Add(newBookBorrowingRequest))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Add(newBookBorrowingRequest);

        // Assert
        var createdResult = result as ObjectResult;
        createdResult.Should().NotBeNull();
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnOkResponse_WhenAdmin()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updatedStatusRequest = new UpdateStatusRequest();
        var expectedResult = Result<object>.SuccessWithBody(new object());

        _bookBorrowingRequestServiceMock.Setup(service => service.UpdateStatus(id, updatedStatusRequest))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.UpdateStatus(id, updatedStatusRequest);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task UpdateExtendedDueDate_ShouldReturnOkResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var extendedDueDate = new ExtendBorrowingRequest();
        var expectedResult = Result<object>.SuccessWithBody(new object());

        _bookBorrowingRequestServiceMock.Setup(service => service.UpdateExtendedDueDate(id, extendedDueDate))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.UpdateExtendedDueDate(id, extendedDueDate);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }
}
