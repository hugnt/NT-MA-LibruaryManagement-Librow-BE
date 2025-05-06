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

public class BookRatingControllerTests
{
    private readonly Mock<IBookRatingService> _bookRatingServiceMock;
    private readonly BookRatingController _controller;

    public BookRatingControllerTests()
    {
        _bookRatingServiceMock = new Mock<IBookRatingService>();
        _controller = new BookRatingController(_bookRatingServiceMock.Object);
    }

    [Fact]
    public async Task GetByBookId_ShouldReturnOkResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookRatingServiceMock.Setup(service => service.GetByBookId(id))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetByBookId(id);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetUserRight_ShouldReturnOkResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookRatingServiceMock.Setup(service => service.GetUserRight(id))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetUserRight(id);

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
        var newBookRating = new BookRatingRequest();
        var expectedResult = Result.Success(HttpStatusCode.Created, "Book rating added successfully");

        _bookRatingServiceMock.Setup(service => service.Add(newBookRating))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Add(newBookRating);

        // Assert
        var createdResult = result as ObjectResult;
        createdResult.Should().NotBeNull();
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenErrorOccurs()
    {
        // Arrange
        var newBookRating = new BookRatingRequest();
        var expectedResult = Result.ErrorWithMessage("Failed to add book rating");

        _bookRatingServiceMock.Setup(service => service.Add(newBookRating))
                            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Add(newBookRating);

        // Assert
        var badRequestResult = result as ObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(expectedResult);
    }
}
