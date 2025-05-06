using FluentAssertions;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Application.Validators;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Librow.Infrastructure.Repositories.Implement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookCategoryServiceTests;
public class DeleteTests
{
    private readonly Mock<IBookCategoryRepository> _mockBookCategoryRepository;
    private readonly Mock<IRepository<Book>> _mockBookRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessorRepository;
    private readonly BookCategoryService _bookCategoryService;
    private readonly BookCategoryValidator _bookCategoryValidator;
    public DeleteTests()
    {
        _mockBookCategoryRepository = new Mock<IBookCategoryRepository>();
        _mockBookRepository = new Mock<IRepository<Book>>();
        _mockHttpContextAccessorRepository = new Mock<IHttpContextAccessor>();
        _bookCategoryValidator = new BookCategoryValidator();
        _bookCategoryService = new BookCategoryService(_mockBookCategoryRepository.Object, _bookCategoryValidator, _mockBookRepository.Object, _mockHttpContextAccessorRepository.Object);

        //Arrange
        var mockCategories = MockBookCategoryRepositorySetup.ListCategories();

        _mockBookCategoryRepository.Setup(x => x.FirstOrDefaultAsync(
                      It.IsAny<Expression<Func<BookCategory, bool>>>(),
                      It.IsAny<CancellationToken>(),
                      It.IsAny<Expression<Func<BookCategory, object>>[]>()
                    )).ReturnsAsync((Expression<Func<BookCategory, bool>> predicate,
                                    CancellationToken token,
                                    Expression<Func<BookCategory, object>>[] nav) =>
                                    {
                                        var data = mockCategories.FirstOrDefault(predicate.Compile());
                                        return data;
                                    });

        _mockBookCategoryRepository.Setup(repo => repo.GetDefaultCategory()).ReturnsAsync(MockBookCategoryRepositorySetup.DefaultCategory);

    }

    [Theory]
    [InlineData("11111111-0000-1111-0000-111111111111")]
    public async Task Delete_NotExistedId_ReturnNotFound(Guid id)
    {
        // Act
        var result = await _bookCategoryService.Delete(id);

        // Assert
        result.Should().BeOfType<Result>();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_CannotDeleteDefaultCategory_ReturnsBadRequest()
    {
        // Arrange
        var categoryId = MockBookCategoryRepositorySetup.DefaultCategory.Id;

        // Act
        var result = await _bookCategoryService.Delete(categoryId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_Success_ReturnsNoContent()
    {
        // Arrange
        _mockBookCategoryRepository.Setup(repo => repo.Delete(It.IsAny<BookCategory>()));

        _mockBookRepository.Setup(repo => repo.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockBookRepository.Setup(repo => repo.ExecuteUpdateAsync(
                        It.IsAny<Expression<Func<Book, bool>>>(), 
                        It.IsAny<Expression<Func<SetPropertyCalls<Book>, SetPropertyCalls<Book>>>>(),
                        It.IsAny<CancellationToken>()
                    )).Returns(Task.CompletedTask);
        _mockBookRepository.Setup(repo => repo.CommitAsync()).Returns(Task.CompletedTask);
        _mockBookRepository.Setup(repo => repo.RollbackAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _bookCategoryService.Delete(MockBookCategoryRepositorySetup.PhilosophyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _mockBookCategoryRepository.Verify(r => r.Delete(It.IsAny<BookCategory>()), Times.Once);
        _mockBookCategoryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_InternalServerError_ReturnsError()
    {
        // Arrange
        _mockBookCategoryRepository.Setup(repo => repo.Delete(It.IsAny<BookCategory>()));

        _mockBookRepository.Setup(repo => repo.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockBookRepository.Setup(repo => repo.ExecuteUpdateAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Expression<Func<SetPropertyCalls<Book>, SetPropertyCalls<Book>>>>(),
            It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Something went wrong"));

        _mockBookRepository.Setup(repo => repo.RollbackAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _bookCategoryService.Delete(MockBookCategoryRepositorySetup.PhilosophyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

}
