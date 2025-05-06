using FluentAssertions;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Application.Validators;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookServiceTests;
public class UpdateTests
{
    private readonly Mock<IRepository<Book>> _mockBookRepository;
    private readonly Mock<IRepository<BookCategory>> _mockBookCategoryRepository;
    private readonly BookService _bookService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessorRepository;
    private readonly BookValidator _bookValidator;
    List<Book> MockBooks = MockBookRepositorySetup.ListBooks();
    List<BookCategory> MockCategories = MockBookCategoryRepositorySetup.ListCategories();

    public UpdateTests()
    {
        _mockBookRepository = new Mock<IRepository<Book>>();
        _mockBookCategoryRepository = new Mock<IRepository<BookCategory>>();
        _mockHttpContextAccessorRepository = new Mock<IHttpContextAccessor>();
        _bookValidator = new BookValidator();
        _bookService = new BookService(_mockBookRepository.Object, _bookValidator, _mockBookCategoryRepository.Object, _mockHttpContextAccessorRepository.Object, null!);


        _mockBookRepository.Setup(x => x.FirstOrDefaultAsync(
                      It.IsAny<Expression<Func<Book, bool>>>(),
                      It.IsAny<CancellationToken>(),
                      It.IsAny<Expression<Func<Book, object>>[]>()
                    )).ReturnsAsync((Expression<Func<Book, bool>> predicate,
                                    CancellationToken token,
                                    Expression<Func<Book, object>>[] nav) =>
                    {
                        var data = MockBooks.FirstOrDefault(predicate.Compile());
                        return data;
                    });

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

    [Theory]
    [InlineData("11111111-0000-1111-0000-111111111111")]
    public async Task Update_NotExistedId_ReturnNotFound(Guid id)
    {
        // Act
        var result = await _bookService.Update(id, It.IsAny<BookRequest>());

        // Assert
        result.Should().BeOfType<Result>();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_InvalidModel_ReturnsValidationError()
    {
        // Arrange
        var invalidRequest = new BookRequest { Title = "", Author = "", Quantity = -1 };

        // Act
        var result = await _bookService.Update(MockBooks.First().Id, invalidRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_QuantityLowerThanBorrowed_ReturnsBadRequest()
    {
        // Arrange
        var updateRequest = new BookRequest 
        { 
            Title = "Book",
            Author = "Author", 
            Quantity = 2, 
            CategoryId = MockBookCategoryRepositorySetup.MathId
        };

        // Act
        var result = await _bookService.Update(MockBooks.First().Id, updateRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(BookMessage.QuantityCanNotBeLowerThanBorrowingNumber);
    }

    [Fact]
    public async Task Update_BookCategoryNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new BookRequest { Title = "Book", Author = "Author", Quantity = 6, CategoryId = Guid.NewGuid() };

        // Act
        var result = await _bookService.Update(MockBooks.First().Id, updateRequest);

        // Assert
        result.Should().BeOfType<Result>();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);

    }

    [Fact]
    public async Task Update_ValidBook_ReturnsNoContent()
    {
        // Arrange
        var updateRequest = new BookRequest
        {
            Title = "Updated Book",
            Author = "Updated Author",
            Quantity = 6,
            CategoryId = MockBookCategoryRepositorySetup.MathId
        };

        _mockBookRepository.Setup(r => r.Update(It.IsAny<Book>()));
        _mockBookRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
                new Claim(ClaimType.Id, Guid.NewGuid().ToString())
        }));
        _mockHttpContextAccessorRepository.Setup(x => x.HttpContext!.User).Returns(user);

        // Act
        var result = await _bookService.Update(MockBooks.First().Id, updateRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _mockBookRepository.Verify(r => r.Update(It.IsAny<Book>()), Times.Once);
        _mockBookRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
