using FluentAssertions;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;
public class GetAllTests
{
    private readonly Mock<IBookBorrowingRequestRepository> _mockBookBorrowingRequestRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessorRepository;
    private readonly BookBorrowingRequestService _bookBorrowingRequestService;
    List<BookBorrowingRequest> MockRequests = MockBookBorrowingRequestRepositorySetup.ListBorrowingRequests();
    public GetAllTests()
    {
        _mockBookBorrowingRequestRepository = new Mock<IBookBorrowingRequestRepository>();
        _mockHttpContextAccessorRepository = new Mock<IHttpContextAccessor>();
        _bookBorrowingRequestService = new BookBorrowingRequestService(null!, _mockHttpContextAccessorRepository.Object, null!, _mockBookBorrowingRequestRepository.Object, null!, null!);

        _mockBookBorrowingRequestRepository.Setup(r =>
                        r.GetByFilterAsync(
                                    It.IsAny<int?>(),
                                    It.IsAny<int?>(),
                                    It.IsAny<Expression<Func<BookBorrowingRequest, BorrowingRequestResponse>>>(),
                                    It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(),
                                    It.IsAny<CancellationToken>(),
                                    It.IsAny<Expression<Func<BookBorrowingRequest, object>>[]>()
                        )).ReturnsAsync((
                                    int? pageSize,
                                    int? pageNumber,
                                    Expression<Func<BookBorrowingRequest, BorrowingRequestResponse>> selectQuery,
                                    Expression<Func<BookBorrowingRequest, bool>> predicate,
                                    CancellationToken token,
                                    Expression<Func<BookBorrowingRequest, object>>[] navigationProperties) =>
                        {
                            var filteredBooks = MockRequests.Where(predicate.Compile()).ToList();

                            var pagedBooks = pageSize.HasValue && pageNumber.HasValue
                                ? filteredBooks.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value)
                                : filteredBooks;

                            var projected = pagedBooks.Select(selectQuery.Compile()).ToList();

                            return (projected, filteredBooks.Count);
                        });
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsFilteredRequests()
    {
        // Arrange
        var filter = new BorrowingRequestFilter { Status = RequestStatus.Waiting, PageSize = 10, PageNumber = 1 };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
              new Claim(ClaimType.Role, Role.Admin.ToString())
        }));
        _mockHttpContextAccessorRepository.Setup(x => x.HttpContext!.User).Returns(user);

        // Act
        var result = await _bookBorrowingRequestService.GetAll(filter);


        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var filterResult = result.As<FilterResult<List<BorrowingRequestResponse>>>();
        filterResult.Data.Should().HaveCount(2);
        filterResult.TotalRecords.Should().Be(2);

    }

    [Fact]
    public async Task GetAll_AsCustomer_ReturnsOwnFilteredRequests()
    {
        // Arrange
        var customerId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"); // user001

        var filter = new BorrowingRequestFilter
        {
            Status = RequestStatus.Waiting,
            PageSize = 10,
            PageNumber = 1
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Role, Role.Customer.ToString()),
            new Claim(ClaimType.Id, customerId.ToString())
        }));

        _mockHttpContextAccessorRepository.Setup(x => x.HttpContext!.User).Returns(user);

        // Act
        var result = await _bookBorrowingRequestService.GetAll(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var filterResult = result.As<FilterResult<List<BorrowingRequestResponse>>>();

        // user001 có 2 request status = Waiting
        filterResult.Data.Should().HaveCount(1);
        filterResult.TotalRecords.Should().Be(1);

        filterResult.Data.Should().OnlyContain(x => x.Status == RequestStatus.Waiting);
    }

}
