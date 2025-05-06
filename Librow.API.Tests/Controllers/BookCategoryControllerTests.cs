using FluentAssertions;
using Librow.API.Controllers;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace Librow.API.Tests.Controllers;

public class BookCategoryControllerTests
{
    private readonly Mock<IBookCategoryService> _bookCategoryServiceMock;
    private readonly BookCategoryController _controller;

    public BookCategoryControllerTests()
    {
        _bookCategoryServiceMock = new Mock<IBookCategoryService>();
        _controller = new BookCategoryController(_bookCategoryServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResponse()
    {
        // Arrange
        var filter = new FilterRequest();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookCategoryServiceMock.Setup(service => service.GetAll(filter))
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
    public async Task GetById_ShouldReturnOkResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

        _bookCategoryServiceMock.Setup(service => service.GetById(id))
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
        var newBookCategory = new BookCategoryRequest();
        var expectedResult = Result.Success(HttpStatusCode.Created,"Book Category added successfully");

        _bookCategoryServiceMock.Setup(service => service.Add(newBookCategory))
                                .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Add(newBookCategory);

        // Assert
        var createdResult = result as ObjectResult;
        createdResult.Should().NotBeNull();
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Update_ShouldReturnOkResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updatedBookCategory = new BookCategoryRequest();
        var expectedResult = Result<object>.SuccessWithBody(new object()); 

        _bookCategoryServiceMock.Setup(service => service.Update(id, updatedBookCategory))
                                .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(id, updatedBookCategory);

        // Assert
        var okResult = result as ObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContentResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result.SuccessNoContent();

        _bookCategoryServiceMock.Setup(service => service.Delete(id))
                                .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFoundResponse_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = Result.ErrorNotFound("Book Category not found");

        _bookCategoryServiceMock.Setup(service => service.GetById(id))
                                .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var notFoundResult = result as ObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult.StatusCode.Should().Be(404);
        notFoundResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Add_ShouldReturnBadRequest_WhenErrorOccurs()
    {
        // Arrange
        var newBookCategory = new BookCategoryRequest();
        var expectedResult = Result.ErrorWithMessage("Failed to add Book Category");

        _bookCategoryServiceMock.Setup(service => service.Add(newBookCategory))
                                .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Add(newBookCategory);

        // Assert
        var badRequestResult = result as ObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(expectedResult);
    }
}