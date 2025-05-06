using FluentAssertions;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;

public class GetUserRequestInfoTests
{
    private readonly Mock<IBookBorrowingRequestRepository> _mockRequestRepo;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly BookBorrowingRequestService _service;

    public GetUserRequestInfoTests()
    {
        _mockRequestRepo = new Mock<IBookBorrowingRequestRepository>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _service = new BookBorrowingRequestService(null!, _mockHttpContextAccessor.Object, null!, _mockRequestRepo.Object, null!, null!);

        _mockRequestRepo.Setup(x => x.CountAsync(
                        It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<Expression<Func<BookBorrowingRequest, object>>[]>()))
                    .ReturnsAsync((Expression<Func<BookBorrowingRequest, bool>> predicate,
                                   CancellationToken token,
                                   Expression<Func<BookBorrowingRequest, object>>[] navigationProperties) =>
                    {
                        var compiledPredicate = predicate.Compile();
                        var matchingCount = MockBookBorrowingRequestRepositorySetup.ListBorrowingRequests().Count(compiledPredicate);
                        return matchingCount;
                    });

    }

    [Fact]
    public async Task GetUserRequestInfo_ReturnsCorrectCountAndMaxLimit()
    {
        // Arrange
        var userId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d");
        var today = DateTime.Today;
        var start = new DateTime(today.Year, today.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var claimsUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Id, userId.ToString())
        }));
        _mockHttpContextAccessor.Setup(x => x.HttpContext!.User).Returns(claimsUser);

        var requestFilter = new RequestFilter
        {
            StartDate = start,
            EndDate = end
        };

        // Act
        var result = await _service.GetUserRequestInfo(requestFilter);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var body = result.As<Result<RequestFilterResponse>>();
        body.Data.TotalRequest.Should().Be(1);
        body.Data.MaxRequestPerMonth.Should().Be(3); 
    }
}
