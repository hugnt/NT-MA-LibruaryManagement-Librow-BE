using FluentAssertions;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Moq;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Linq.Expressions;

namespace Librow.Application.Tests.Services.BookServiceTests;

public class GetAllTests
{

    private readonly Mock<IRepository<Book>> _mockBookRepository;
    private readonly BookService _bookService;
    List<Book> MockBooks = MockBookRepositorySetup.ListBooks();
    public GetAllTests()
    {
        _mockBookRepository = new Mock<IRepository<Book>>();
        _bookService = new BookService(_mockBookRepository.Object, null!, null!, null!, null!);

        _mockBookRepository.Setup(r =>
                        r.GetByFilterAsync(
                                    It.IsAny<int?>(),
                                    It.IsAny<int?>(),
                                    It.IsAny<Expression<Func<Book, BookResponse>>>(),
                                    It.IsAny<Expression<Func<Book, bool>>>(),
                                    It.IsAny<CancellationToken>(),
                                    It.IsAny<Expression<Func<Book, object>>[]>()
                        )).ReturnsAsync((
                                    int? pageSize,
                                    int? pageNumber,
                                    Expression<Func<Book, BookResponse>> selectQuery,
                                    Expression<Func<Book, bool>> predicate,
                                    CancellationToken token,
                                    Expression<Func<Book, object>>[] navigationProperties) =>
                        {
                            var filteredBooks = MockBooks.Where(predicate.Compile()).ToList();

                            var pagedBooks = pageSize.HasValue && pageNumber.HasValue
                                ? filteredBooks.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value)
                                : filteredBooks;

                            var projected = pagedBooks.Select(selectQuery.Compile()).ToList();

                            return (projected, filteredBooks.Count);
                        });

    }

    [Fact]
    public async Task GetByFilter_NoFilter_ReturnsAllRecords()
    {
        // Arrange
        var filter = new BookFilterRequest();

        // Act
        var result = await _bookService.GetAll(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var filterResult = result.As<FilterResult<List<BookResponse>>>();
        filterResult.Data.Should().HaveCount(MockBooks.Count());
        filterResult.TotalRecords.Should().Be(MockBooks.Count());
    }


    [Fact]
    public async Task GetByFilter_WithPagination_ReturnsPagedRecords()
    {
        // Arrange
        var filter = new BookFilterRequest
        {
            PageSize = 2,
            PageNumber = 2
        };

        // Act
        var result = await _bookService.GetAll(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var filterResult = result.As<FilterResult<List<BookResponse>>>();
        filterResult.Data.Should().HaveCount(2);
        filterResult.TotalRecords.Should().Be(MockBooks.Count());
    }


}
