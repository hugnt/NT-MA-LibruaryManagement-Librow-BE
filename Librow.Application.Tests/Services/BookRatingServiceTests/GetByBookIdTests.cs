using FluentAssertions;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Linq.Expressions;
using FluentValidation;
using Librow.Application.Models;

namespace Librow.Application.Tests.Services.BookRatingServiceTests;

public class GetByBookIdTests
{
    private readonly Mock<IRepository<Book>> _bookRepoMock = new();
    private readonly Mock<IRepository<BookRating>> _bookRatingRepoMock = new();
    private readonly BookRatingService _service;

    public GetByBookIdTests()
    {
        _service = new BookRatingService(
            Mock.Of<IValidator<BookRatingRequest>>(),
            _bookRepoMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IRepository<BookBorrowingRequest>>(),
            _bookRatingRepoMock.Object,
            Mock.Of<IRepository<User>>()
        );
    }

    [Fact]
    public async Task GetByBookId_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>(), default))
                     .ReturnsAsync(false);

        // Act
        var result = await _service.GetByBookId(bookId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByBookId_ShouldReturnEmptyReviewList_WhenNoRatingsExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>(), default))
                     .ReturnsAsync(true);
        _bookRatingRepoMock.Setup(x => x.GetAllAsync(
                It.IsAny<Expression<Func<BookRating, bool>>>(),
                It.IsAny<Expression<Func<BookRating, ReviewModel>>>(),
                default,
                It.IsAny<Expression<Func<BookRating, object>>>()
            ))
            .ReturnsAsync(new List<ReviewModel>());

        // Act
        var result = await _service.GetByBookId(bookId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<Result<BookRatingResponse>>();
        var typedResult = result as Result<BookRatingResponse>;
        typedResult.Data.Reviews.Should().BeEmpty();
        typedResult.Data.AverageRating.Should().Be(0);
    }

    [Fact]
    public async Task GetByBookId_ShouldReturnAverageAndReviews_WhenRatingsExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var reviews = new List<ReviewModel>
        {
            new() { Id = Guid.NewGuid(), ReviewerName = "Alice", Comment = "Great", Rate = 4 },
            new() { Id = Guid.NewGuid(), ReviewerName = "Bob", Comment = "Good", Rate = 5 }
        };

        _bookRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>(), default))
                     .ReturnsAsync(true);

        _bookRatingRepoMock.Setup(x => x.GetAllAsync(
                It.IsAny<Expression<Func<BookRating, bool>>>(),
                It.IsAny<Expression<Func<BookRating, ReviewModel>>>(),
                default,
                It.IsAny<Expression<Func<BookRating, object>>>()
            ))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.GetByBookId(bookId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var typedResult = result as Result<BookRatingResponse>;
        typedResult.Data.Reviews.Should().HaveCount(2);
        typedResult.Data.AverageRating.Should().BeApproximately(4.5, 0.01);
    }
}
