using FluentAssertions;
using FluentAssertions.Execution;
using Librow.Application.Models;
using Librow.Application.Models.Mappings;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Tests.Services.BookCategoryServiceTests;
public class GetByIdTests
{
    private readonly Mock<IBookCategoryRepository> _mockBookCategoryRepository;
    private readonly BookCategoryService _bookCategoryService;
    List<BookCategory> MockCategories = MockBookCategoryRepositorySetup.ListCategories();
    public GetByIdTests()
    {
        _mockBookCategoryRepository = new Mock<IBookCategoryRepository>();
        _bookCategoryService = new BookCategoryService(_mockBookCategoryRepository.Object, null!, null!, null!);
        _mockBookCategoryRepository.Setup(x => x.FirstOrDefaultAsync(
                          It.IsAny<Expression<Func<BookCategory, bool>>>(),
                          It.IsAny<Expression<Func<BookCategory, BookCategoryResponse>>>(),
                          It.IsAny<CancellationToken>(),
                          It.IsAny<Expression<Func<BookCategory, object>>[]>()
              )).ReturnsAsync((Expression<Func<BookCategory, bool>> predicate,
                               Expression<Func<BookCategory, BookCategoryResponse>> selector,
                               CancellationToken token,
                               Expression<Func<BookCategory, object>>[] nav) =>
              {
                  var data = MockCategories.AsQueryable().Where(predicate).Select(selector).FirstOrDefault();
                  return data;
              });

    }


    [Theory]
    [InlineData("11111111-0000-1111-0000-111111111111")]
    [InlineData("11111111-2222-1111-2222-111111111111")]
    public async Task GetById_NotExistedId_ReturnNotFound(Guid id)
    {
        // Arrange
        var mockCategories = MockBookCategoryRepositorySetup.ListCategories();

        // Act
        var result = await _bookCategoryService.GetById(id);

        // Assert
        result.Should().BeOfType<Result>();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    public async Task GetById_ValidId_ReturnBookCategory(Guid id)
    {
        // Act
        var result = await _bookCategoryService.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Result<BookCategoryResponse>>();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = result as Result<BookCategoryResponse>;
        response!.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(id);

    }
}
