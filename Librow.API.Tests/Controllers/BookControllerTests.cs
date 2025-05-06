using FluentAssertions;
using Librow.API.Controllers;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using Xunit;

namespace Librow.API.Tests.Controllers
{
    public class BookControllerTests
    {
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly BookController _controller;

        public BookControllerTests()
        {
            _bookServiceMock = new Mock<IBookService>();
            _controller = new BookController(_bookServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResponse()
        {
            // Arrange
            var filter = new BookFilterRequest();
            var expectedResult = Result<object>.SuccessWithBody(new object()); // Giả lập dữ liệu trả về

            _bookServiceMock.Setup(service => service.GetAll(filter))
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

            _bookServiceMock.Setup(service => service.GetById(id))
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
            var newBook = new BookRequest();
            var expectedResult = Result.Success(HttpStatusCode.Created, "Book added successfully");

            _bookServiceMock.Setup(service => service.Add(newBook))
                            .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Add(newBook);

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
            var updatedBook = new BookRequest();
            var expectedResult = Result<object>.SuccessWithBody(new object());

            _bookServiceMock.Setup(service => service.Update(id, updatedBook))
                            .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Update(id, updatedBook);

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

            _bookServiceMock.Setup(service => service.Delete(id))
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
            var expectedResult = Result.ErrorNotFound("Book not found");

            _bookServiceMock.Setup(service => service.GetById(id))
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
            var newBook = new BookRequest();
            var expectedResult = Result.ErrorWithMessage("Failed to add book");

            _bookServiceMock.Setup(service => service.Add(newBook))
                            .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Add(newBook);

            // Assert
            var badRequestResult = result as ObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be(expectedResult);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenBookNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedResult = Result.ErrorNotFound("Book not found");

            _bookServiceMock.Setup(service => service.Delete(id))
                            .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = result as ObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be(expectedResult);
        }

   
    }
}
