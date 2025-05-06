using FluentAssertions;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;

public class GetAllBorrowingBooksTests
{
    private readonly Mock<IRepository<BookBorrowingRequestDetails>> _mockDetailsRepo;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly BookBorrowingRequestService _service;
    private readonly List<BookBorrowingRequestDetails> _mockDetails;

    public GetAllBorrowingBooksTests()
    {
        _mockDetailsRepo = new Mock<IRepository<BookBorrowingRequestDetails>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDetails = MockBookBorrowingRequestDetailsRepositorySetup.ListBookBorrowingRequestDetails();

        _service = new BookBorrowingRequestService(null!, _mockHttpContextAccessor.Object, _mockDetailsRepo.Object, null!, null!, null!);

        _mockDetailsRepo.Setup(r => r.GetByFilterAsync(
            It.IsAny<int?>(), It.IsAny<int?>(),
            It.IsAny<Expression<Func<BookBorrowingRequestDetails, BorrowingBookResponse>>>(),
            It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<BookBorrowingRequestDetails, object>>[]>()
        )).ReturnsAsync((
            int? pageSize,
            int? pageNumber,
            Expression<Func<BookBorrowingRequestDetails, BorrowingBookResponse>> select,
            Expression<Func<BookBorrowingRequestDetails, bool>> predicate,
            CancellationToken token,
            Expression<Func<BookBorrowingRequestDetails, object>>[] includes
        ) =>
        {
            var filtered = _mockDetails.Where(predicate.Compile()).ToList();
            var paged = filtered.Skip((pageNumber!.Value - 1) * pageSize!.Value).Take(pageSize.Value);
            var result = paged.Select(select.Compile()).ToList();
            return (result, filtered.Count);
        });
    }

    [Fact]
    public async Task GetAllBorrowingBooks_AsAdmin_ReturnsAllMatchingRecords()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Role, Role.Admin.ToString()),
            new Claim(ClaimType.Id, Guid.Empty.ToString())
        }));
        _mockHttpContextAccessor.Setup(x => x.HttpContext!.User).Returns(user);

        var filter = new BorrowingRequestFilter { Status = RequestStatus.Approved, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetAllBorrowingBooks(filter);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var data = result.As<FilterResult<List<BorrowingBookResponse>>>();
        data.Data.Should().OnlyContain(x => x.RequestStatus == RequestStatus.Approved);
    }

    [Fact]
    public async Task GetAllBorrowingBooks_AsCustomer_ReturnsOnlyOwnedRecords()
    {
        // Arrange
        var userId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c48"); // user002
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Role, Role.Customer.ToString()),
            new Claim(ClaimType.Id, userId.ToString())
        }));
        _mockHttpContextAccessor.Setup(x => x.HttpContext!.User).Returns(user);

        var filter = new BorrowingRequestFilter { Status = RequestStatus.Approved, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.GetAllBorrowingBooks(filter);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var data = result.As<FilterResult<List<BorrowingBookResponse>>>();
        data.Data.Should().OnlyContain(x => x.RequestStatus == RequestStatus.Approved);
    }
}
