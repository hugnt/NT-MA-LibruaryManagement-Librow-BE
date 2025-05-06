using FluentValidation;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models.Requests;
using Librow.Application.Models;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FluentAssertions;

namespace Librow.Application.Tests.Services.BookRatingServiceTests;
public class GetUserRightTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly Mock<IRepository<Book>> _bookRepoMock = new();
    private readonly Mock<IRepository<BookBorrowingRequest>> _requestRepoMock = new();
    private readonly IHttpContextAccessor _httpContextAccessorMock;
    private readonly BookRatingService _service;

    public GetUserRightTests()
    {
        var userId = Guid.NewGuid();
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Id, userId.ToString())
        }, "mock"));

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>().Object;
        _httpContextAccessorMock.HttpContext = new DefaultHttpContext
        {
            User = userClaims
        };

        _service = new BookRatingService(
            Mock.Of<IValidator<BookRatingRequest>>(),
            _bookRepoMock.Object,
            _httpContextAccessorMock,
            _requestRepoMock.Object,
            Mock.Of<IRepository<BookRating>>(),
            _userRepoMock.Object
        );
    }

    [Fact]
    public async Task GetUserRight_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
                     .ReturnsAsync(false);

        // Act
        var result = await _service.GetUserRight(bookId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var typedResult = result as Result<bool>;
        typedResult.Data.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserRight_ShouldReturnFalse_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
                     .ReturnsAsync(true);
        _bookRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>(), default))
                     .ReturnsAsync(false);

        // Act
        var result = await _service.GetUserRight(bookId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var typedResult = result as Result<bool>;
        typedResult.Data.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserRight_ShouldReturnFalse_WhenUserHasNoBorrowRequestWithBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
                     .ReturnsAsync(true);
        _bookRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>(), default))
                     .ReturnsAsync(true);
        _requestRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default))
                        .ReturnsAsync(false);

        // Act
        var result = await _service.GetUserRight(bookId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (result as Result<bool>).Data.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserRight_ShouldReturnTrue_WhenUserHasBorrowedBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
                     .ReturnsAsync(true);
        _bookRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>(), default))
                     .ReturnsAsync(true);
        _requestRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default))
                        .ReturnsAsync(true);

        // Act
        var result = await _service.GetUserRight(bookId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (result as Result<bool>).Data.Should().BeTrue();
    }
}
