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
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookCategoryServiceTests;
public class AddTests
{
    private readonly Mock<IBookCategoryRepository> _mockBookCategoryRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessorRepository;
    private readonly BookCategoryService _bookCategoryService;
    private readonly BookCategoryValidator _bookCategoryValidator;
    public AddTests()
    {
        _mockBookCategoryRepository = new Mock<IBookCategoryRepository>();
        _mockHttpContextAccessorRepository = new Mock<IHttpContextAccessor>();
        _bookCategoryValidator = new BookCategoryValidator();
        _bookCategoryService = new BookCategoryService(_mockBookCategoryRepository.Object, _bookCategoryValidator, null!, _mockHttpContextAccessorRepository.Object);

        var mockCategories = MockBookCategoryRepositorySetup.ListCategories();
        _mockBookCategoryRepository.Setup(r => r.AnyAsync(
               It.IsAny<Expression<Func<BookCategory, bool>>>(),
               It.IsAny<CancellationToken>(),
               It.IsAny<Expression<Func<BookCategory, object>>[]>()))
         .ReturnsAsync((Expression<Func<BookCategory, bool>> predicate,
                            CancellationToken token,
                            Expression<Func<BookCategory, object>>[] nav) =>
         {
             var data = mockCategories.Any(predicate.Compile());
             return data;
         });

    }

    [Fact]
    public async Task Add_EmptyName_ReturnsInvalidResult()
    {
        // Arrange
        var bookCategory = new BookCategoryRequest { Name = string.Empty };

        // Act
        var result = await _bookCategoryService.Add(bookCategory);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Science")]
    [InlineData("Technology")]
    public async Task Add_DuplicateCategory_ReturnsBadRequest(string categoryName)
    {
        // Arrange
        var bookCategory = new BookCategoryRequest { Name = categoryName };

        // Act
        var result = await _bookCategoryService.Add(bookCategory);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(ErrorMessage.ObjectExisted(categoryName, "Book Category"));
    }

    [Fact]
    public async Task Add_ValidBookCategory_ReturnsSuccess()
    {
        // Arrange
        var newCategory = new BookCategoryRequest { Name = "New Category" };

        _mockBookCategoryRepository.Setup(r => r.Add(It.IsAny<BookCategory>()));
        _mockBookCategoryRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));

        var user = new ClaimsPrincipal(new ClaimsIdentity(new []
        {
                new Claim(ClaimType.Id, Guid.NewGuid().ToString())
        }));
        _mockHttpContextAccessorRepository.Setup(x => x.HttpContext!.User).Returns(user);

        // Act
        var result = await _bookCategoryService.Add(newCategory);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<Result>();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Message.Should().Be(SuccessMessage.CreatedSuccessfully("Book Category"));

        _mockBookCategoryRepository.Verify(r => r.Add(It.IsAny<BookCategory>()), Times.Once);
        _mockBookCategoryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


}
