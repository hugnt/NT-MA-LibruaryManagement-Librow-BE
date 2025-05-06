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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookServiceTests;
public class DeleteTests
{
    private readonly Mock<IRepository<Book>> _mockBookRepository;
    private readonly BookService _bookService;
    private readonly Mock<IRepository<BookBorrowingRequestDetails>> _mockBookBorrowingRequestDetailsRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessorRepository;
    private readonly BookValidator _bookValidator;
    List<Book> MockBooks = MockBookRepositorySetup.ListBooks();
    List<BookBorrowingRequestDetails> MockBookBorrowingRequestDetails = MockBookBorrowingRequestDetailsRepositorySetup.ListBookBorrowingRequestDetails();
    public DeleteTests()
    {
        _mockBookRepository = new Mock<IRepository<Book>>();
        _mockBookBorrowingRequestDetailsRepository = new Mock<IRepository<BookBorrowingRequestDetails>>();
        _mockHttpContextAccessorRepository = new Mock<IHttpContextAccessor>();
        _bookValidator = new BookValidator();
        _bookService = new BookService(_mockBookRepository.Object, _bookValidator, null!, _mockHttpContextAccessorRepository.Object, _mockBookBorrowingRequestDetailsRepository.Object);


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
        _mockBookBorrowingRequestDetailsRepository.Setup(r => r.AnyAsync(
                       It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(),
                       It.IsAny<CancellationToken>(),
                       It.IsAny<Expression<Func<BookBorrowingRequestDetails, object>>[]>()))
                        .ReturnsAsync((Expression<Func<BookBorrowingRequestDetails, bool>> predicate,
                                    CancellationToken token,
                                    Expression<Func<BookBorrowingRequestDetails, object>>[] nav) =>
                        {
                            var data = MockBookBorrowingRequestDetails.Any(predicate.Compile());
                            return data;
                        });


    }

    [Theory]
    [InlineData("11111111-0000-1111-0000-111111111111")]
    public async Task Delete_BookNotFound_ReturnsNotFound(Guid id)
    {
        // Act
        var result = await _bookService.Delete(id);

        // Assert
        result.Should().BeOfType<Result>();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_BookInUse_ReturnsError()
    {
        // Arrange
        var book = MockBooks.First(x => x.Quantity > x.Available);

        // Act
        var result = await _bookService.Delete(book.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(BookMessage.BookExistedInOtherProcess);
    }

    [Fact]
    public async Task Delete_BookInBorrowRequests_SoftDeletesBook()
    {
        // Arrange
        var bookInRequest = MockBooks[1];
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
                new Claim(ClaimType.Id, Guid.NewGuid().ToString())
        }));
        _mockHttpContextAccessorRepository.Setup(x => x.HttpContext!.User).Returns(user);

        // Act
        var result = await _bookService.Delete(bookInRequest.Id);

        bookInRequest.IsDeleted.Should().BeTrue();
        _mockBookRepository.Verify(r => r.Update(It.IsAny<Book>()), Times.Once);
        _mockBookRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_BookNotInBorrowRequests_DeletesBook()
    {
        // Arrange
        var book = MockBooks.Last();

        // Act
        var result = await _bookService.Delete(book.Id);

        // Assert
        _mockBookRepository.Verify(r => r.Delete(book), Times.Once);
        _mockBookRepository.Verify(r => r.Update(It.IsAny<Book>()), Times.Never);
        _mockBookRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }


}
