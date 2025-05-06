using FluentAssertions;
using Librow.Application.Models;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Moq;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Linq.Expressions;

namespace Librow.Application.Tests.Services.BookCategoryServiceTests;

public class GetAllTests
{

    private readonly Mock<IBookCategoryRepository> _mockBookCategoryRepository;
    private readonly BookCategoryService _bookCategoryService;
    List<BookCategory> MockCategories = MockBookCategoryRepositorySetup.ListCategories();
    public GetAllTests()
    {
        _mockBookCategoryRepository = new Mock<IBookCategoryRepository>();
        _bookCategoryService = new BookCategoryService(_mockBookCategoryRepository.Object, null!, null!, null!);
        _mockBookCategoryRepository.Setup(r =>
                                r.GetByFilterAsync(
                                    It.IsAny<int?>(),
                                    It.IsAny<int?>(),
                                    It.IsAny<Expression<Func<BookCategory, BookCategoryResponse>>>(),
                                    It.IsAny<Expression<Func<BookCategory, bool>>>(),
                                    It.IsAny<CancellationToken>(),
                                    It.IsAny<Expression<Func<BookCategory, object>>[]>()
                                )).ReturnsAsync((int? pageSize, int? pageNumber,
                                                Expression<Func<BookCategory, BookCategoryResponse>> selectQuery,
                                                Expression<Func<BookCategory, bool>>? predicate = null,
                                                CancellationToken token = default,
                                                params Expression<Func<BookCategory, object>>[] navigationProperties) =>
                                {
                                    var allData = MockCategories.Select(selectQuery.Compile());
                                    var projectedQuery = allData;
                                    if (pageSize.HasValue && pageNumber.HasValue)
                                    {
                                        projectedQuery = projectedQuery.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
                                    }
                                    return (projectedQuery.ToList(), allData.Count());
                                });
    }

    [Fact]
    public async Task GetByFilter_NoFilter_ReturnsAllRecords()
    {
        // Arrange
        var filter = new FilterRequest();

        // Act
        var result = await _bookCategoryService.GetAll(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<FilterResult<List<BookCategoryResponse>>>();

        var filterResult = result as FilterResult<List<BookCategoryResponse>>;
        filterResult!.Data.Should().HaveCount(MockCategories.Count);
        filterResult!.TotalRecords.Should().Be(MockCategories.Count);
    }

    [Fact]
    public async Task GetByFilter_WithPagination_ReturnsPagedRecords()
    {
        // Arrange
        var filter = new FilterRequest
        {
            PageSize = 5,
            PageNumber = 2
        };

        // Act
        var result = await _bookCategoryService.GetAll(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<FilterResult<List<BookCategoryResponse>>>();

        var filterResult = result as FilterResult<List<BookCategoryResponse>>;
        filterResult!.Data.Should().HaveCount(5);
        filterResult.TotalRecords.Should().Be(MockCategories.Count); 
    }


}
