using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Librow.Application.Common.Messages;
using Librow.Application.Models.Requests;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace Librow.Application.Tests.Services.BookRatingServiceTests;

public class BookRatingServiceTests
{
    private readonly Mock<IValidator<BookRatingRequest>> _mockValidator;
    private readonly Mock<IRepository<Book>> _mockBookRepository;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IRepository<BookRating>> _mockBookRatingRepository;
    private readonly Mock<IRepository<BookBorrowingRequest>> _mockBookBorrowingRequestRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly BookRatingService _bookRatingService;

    public BookRatingServiceTests()
    {
        _mockValidator = new Mock<IValidator<BookRatingRequest>>();
        _mockBookRepository = new Mock<IRepository<Book>>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockBookRatingRepository = new Mock<IRepository<BookRating>>();
        _mockBookBorrowingRequestRepository = new Mock<IRepository<BookBorrowingRequest>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _bookRatingService = new BookRatingService(
            _mockValidator.Object,
            _mockBookRepository.Object,
            _mockHttpContextAccessor.Object,
            _mockBookBorrowingRequestRepository.Object,
            _mockBookRatingRepository.Object,
            _mockUserRepository.Object);
    }

    [Fact]
    public async Task Add_ShouldReturnError_WhenValidationFails()
    {
        // Arrange
        var ratingRequest = new BookRatingRequest { BookId = Guid.NewGuid(), Rate = 5, Comment = "Great book!" };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Rate", "Rate is required.") });
        _mockValidator.Setup(v => v.Validate(It.IsAny<BookRatingRequest>())).Returns(validationResult);

        // Act
        var result = await _bookRatingService.Add(ratingRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain("Rate is required.");
    }

    [Fact]
    public async Task Add_ShouldReturnError_WhenUserDoesNotHavePermission()
    {
        // Arrange
        var ratingRequest = new BookRatingRequest { BookId = Guid.NewGuid(), Rate = 5, Comment = "Great book!" };
        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.Validate(It.IsAny<BookRatingRequest>())).Returns(validationResult);

        // Mocking repositories
        _mockBookRepository.Setup(b => b.AnyAsync(
                It.IsAny<Expression<Func<Book, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Book, object>>[]>())).ReturnsAsync(true); // Book exists

        _mockUserRepository.Setup(u => u.AnyAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(true); // User exists

        _mockBookBorrowingRequestRepository.Setup(br => br.AnyAsync(
                It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<BookBorrowingRequest, object>>[]>())).ReturnsAsync(false); // User does not have borrowing request

        // Mocking CheckUserRight to simulate no permission

        // Act
        var result = await _bookRatingService.Add(ratingRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        result.Errors.Should().Contain(ErrorMessage.UserHasNoPermission);
    }

    [Fact]
    public async Task Add_ShouldAddRatingAndReturnSuccess_WhenValid()
    {
        // Arrange
        var ratingRequest = new BookRatingRequest { BookId = Guid.NewGuid(), Rate = 5, Comment = "Great book!" };
        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.Validate(It.IsAny<BookRatingRequest>())).Returns(validationResult);

        var userRightCheck = (true, HttpStatusCode.OK, string.Empty);
        _mockBookRepository.Setup(b => b.AnyAsync(
              It.IsAny<Expression<Func<Book, bool>>>(),
              It.IsAny<CancellationToken>(),
              It.IsAny<Expression<Func<Book, object>>[]>())).ReturnsAsync(true);
        _mockUserRepository.Setup(u => u.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), 
            It.IsAny<CancellationToken>(), 
            It.IsAny<Expression<Func<User, object>>[]>())).ReturnsAsync(true);
        _mockBookBorrowingRequestRepository.Setup(br => br.AnyAsync(
                    It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<BookBorrowingRequest, object>>[]>())).ReturnsAsync(true);

        var bookRatingEntity = new BookRating
        {
            Id = Guid.NewGuid(),
            BookId = ratingRequest.BookId,
            Rate = ratingRequest.Rate,
            Comment = ratingRequest.Comment,
            ReviewerId = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _mockBookRatingRepository.Setup(r => r.Add(It.IsAny<BookRating>())).Verifiable();
        _mockBookRatingRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _bookRatingService.Add(ratingRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        _mockBookRatingRepository.Verify(r => r.Add(It.IsAny<BookRating>()), Times.Once);
        _mockBookRatingRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
