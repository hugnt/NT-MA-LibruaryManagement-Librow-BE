using FluentAssertions;
using Librow.Application.Models;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace Librow.Application.Tests.Services.BookServiceTests;
public class GetByIdTests
{
    private readonly Mock<IRepository<Book>> _mockBookRepository;
    private readonly BookService _bookService;
    List<Book> MockBooks = MockBookRepositorySetup.ListBooks();
    public GetByIdTests()
    {
        _mockBookRepository = new Mock<IRepository<Book>>();
        _bookService = new BookService(_mockBookRepository.Object, null!, null!, null!, null!);

        //Arange
        _mockBookRepository.Setup(x => x.FirstOrDefaultAsync(
                          It.IsAny<Expression<Func<Book, bool>>>(),
                          It.IsAny<Expression<Func<Book, BookResponse>>>(),
                          It.IsAny<CancellationToken>(),
                          It.IsAny<Expression<Func<Book, object>>[]>()
              )).ReturnsAsync((Expression<Func<Book, bool>> predicate,
                               Expression<Func<Book, BookResponse>> selector,
                               CancellationToken token,
                               Expression<Func<Book, object>>[] nav) =>
              {
                  var data = MockBooks.AsQueryable().Where(predicate).Select(selector).FirstOrDefault();
                  return data;
              });


    }


    [Theory]
    [InlineData("11111111-0000-1111-0000-111111111111")]
    public async Task GetById_NotExistedId_ReturnNotFound(Guid id)
    {
        // Act
        var result = await _bookService.GetById(id);

        // Assert
        result.Should().BeOfType<Result>();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1")]
    public async Task GetById_ValidId_ReturnBook(Guid id)
    {
        // Act
        var result = await _bookService.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Result<BookResponse>>();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = result as Result<BookResponse>;
        response!.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(id);

    }
}
