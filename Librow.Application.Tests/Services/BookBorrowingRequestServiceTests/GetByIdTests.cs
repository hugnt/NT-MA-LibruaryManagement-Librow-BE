using FluentAssertions;
using FluentAssertions.Execution;
using Librow.Application.Models;
using Librow.Application.Models.Mappings;
using Librow.Application.Models.Requests;
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

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;
public class GetByIdTests
{
    private readonly Mock<IBookBorrowingRequestRepository> _mockBookBorrowingRequestRepository;
    private readonly BookBorrowingRequestService _bookBorrowingRequestService;
    List<BookBorrowingRequest> MockBorrowingRequests = MockBookBorrowingRequestRepositorySetup.ListBorrowingRequests();
    public GetByIdTests()
    {
        _mockBookBorrowingRequestRepository = new Mock<IBookBorrowingRequestRepository>();
        _bookBorrowingRequestService = new BookBorrowingRequestService(null!,null!,null!,_mockBookBorrowingRequestRepository.Object, null!, null!);

        //Arange
        _mockBookBorrowingRequestRepository.Setup(x => x.GetDetailAsync(
                         It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(),
                         It.IsAny<CancellationToken>()
             )).ReturnsAsync((Expression<Func<BookBorrowingRequest, bool>> predicate,
                              CancellationToken token) =>
             {
                 var data = MockBorrowingRequests.AsQueryable().Where(predicate).FirstOrDefault();
                 return data;
             });
    }


    [Theory]
    [InlineData("11111111-0000-1111-0000-111111111111")]
    public async Task GetById_NotExistedId_ReturnNotFound(Guid id)
    {
     
        // Act
        var result = await _bookBorrowingRequestService.GetById(id);

        // Assert
        result.Should().BeOfType<Result>();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("e1111111-1111-1111-1111-111111111111")]
    public async Task GetById_ValidId_ReturnBookBorrowingRequest(Guid id)
    {
        // Act
        var result = await _bookBorrowingRequestService.GetById(id);
       
        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Result<BorrowingRequestDetailsResponse>>();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = result as Result<BorrowingRequestDetailsResponse>;
        response!.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(id);

    }
}
