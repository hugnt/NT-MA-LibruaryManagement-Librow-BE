using FluentAssertions;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Application.Validators;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookServiceTests;
public class AddTests
{
    private readonly Mock<IRepository<Book>> _mockBookRepository;
    private readonly Mock<IRepository<BookCategory>> _mockBookCategoryRepository;
    private readonly BookService _bookService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessorRepository;
    private readonly BookValidator _bookValidator;
    List<BookCategory> MockCategories = MockBookCategoryRepositorySetup.ListCategories();
    public AddTests()
    {
        _mockBookRepository = new Mock<IRepository<Book>>();
        _mockBookCategoryRepository = new Mock<IRepository<BookCategory>>();
        _mockHttpContextAccessorRepository = new Mock<IHttpContextAccessor>();
        _bookValidator = new BookValidator();
        _bookService = new BookService(_mockBookRepository.Object, _bookValidator, _mockBookCategoryRepository.Object, _mockHttpContextAccessorRepository.Object, null!);

        _mockBookCategoryRepository.Setup(r => r.AnyAsync(
               It.IsAny<Expression<Func<BookCategory, bool>>>(),
               It.IsAny<CancellationToken>(),
               It.IsAny<Expression<Func<BookCategory, object>>[]>()))
         .ReturnsAsync((Expression<Func<BookCategory, bool>> predicate,
                        CancellationToken token,
                        Expression<Func<BookCategory, object>>[] nav) =>
         {
             var data = MockCategories.Any(predicate.Compile());
             return data;
         });

    }

    [Fact]
    public async Task Add_InvalidModel_ReturnsValidationError()
    {
        // Arrange
        var invalidRequest = new BookRequest
        {
            Title = "",
            Author = "", 
            Quantity = -5 
        };

        // Act
        var result = await _bookService.Add(invalidRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_BookCategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new BookRequest
        {
            Title = "Test Book",
            Author = "Test Author",
            Quantity = 5,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await _bookService.Add(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Errors.Should().Contain(ErrorMessage.ObjectNotFound(request.CategoryId, "Book Category"));
    }

    [Fact]
    public async Task Add_ValidBook_ReturnsSuccess()
    {
        // Arrange
        var request = new BookRequest
        {
            Title = "Valid Book",
            Author = "Valid Author",
            Quantity = 10,
            CategoryId = MockBookCategoryRepositorySetup.MathId
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new []
        {
                new Claim(ClaimType.Id, Guid.NewGuid().ToString())
        }));
        _mockHttpContextAccessorRepository.Setup(x => x.HttpContext!.User).Returns(user);

        // Act
        var result = await _bookService.Add(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Message.Should().Be(SuccessMessage.CreatedSuccessfully("Book"));

        _mockBookRepository.Verify(r => r.Add(It.IsAny<Book>()), Times.Once);
        _mockBookRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


}
