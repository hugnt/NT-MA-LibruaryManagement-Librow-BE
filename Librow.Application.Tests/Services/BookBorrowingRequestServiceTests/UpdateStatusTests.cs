using FluentAssertions;
using Librow.Application.Common.Email;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models.Requests;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories.Base;
using Librow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using Librow.Application.Models.Responses;

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;

public class UpdateStatusTests
{
    private readonly Mock<IBookBorrowingRequestRepository> _mockRequestRepo;
    private readonly Mock<IRepository<BookBorrowingRequestDetails>> _mockDetailRepo;
    private readonly Mock<IRepository<Book>> _mockBookRepo;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContext;
    private readonly BookBorrowingRequestService _service;
    private readonly Guid _adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public UpdateStatusTests()
    {
        _mockRequestRepo = new Mock<IBookBorrowingRequestRepository>();
        _mockDetailRepo = new Mock<IRepository<BookBorrowingRequestDetails>>();
        _mockBookRepo = new Mock<IRepository<Book>>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockEmailService = new Mock<IEmailService>();
        _mockHttpContext = new Mock<IHttpContextAccessor>();

        var adminClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Id, _adminId.ToString()),
            new Claim(ClaimType.Role, Role.Admin.ToString())
        }));
        _mockHttpContext.Setup(x => x.HttpContext!.User).Returns(adminClaims);

        _service = new BookBorrowingRequestService(_mockBookRepo.Object, _mockHttpContext.Object, _mockDetailRepo.Object, _mockRequestRepo.Object, _mockEmailService.Object, _mockUserRepo.Object);
    }

    [Fact]
    public async Task UpdateStatus_RequestNotFound_ReturnsNotFound()
    {
        var requestId = Guid.NewGuid();
        _mockRequestRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync((BookBorrowingRequest)null!);

        var result = await _service.UpdateStatus(requestId, new UpdateStatusRequest { Status = RequestStatus.Rejected });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Errors.Should().Contain(ErrorMessage.ObjectNotFound(requestId, "Borrowing Request "));
    }

    [Fact]
    public async Task UpdateStatus_Rejected_IncreasesBookAvailability()
    {
        var requestId = Guid.NewGuid();
        var borrowingRequest = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Waiting
        };

        var bookIds = MockBookRepositorySetup.ListBooks().Select(x => x.Id).Take(2).ToList();

        _mockRequestRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync(borrowingRequest);
        _mockDetailRepo.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(),
                                                It.IsAny<Expression<Func<BookBorrowingRequestDetails, Guid>>>(),
                                                 It.IsAny<CancellationToken>(),
                                                 It.IsAny<Expression<Func<BookBorrowingRequestDetails, object>>[]>()
                                                 )).ReturnsAsync(bookIds);
        _mockBookRepo.Setup(x => x.ExecuteUpdateAsync(It.IsAny<Expression<Func<Book, bool>>>(), 
                                                        It.IsAny<Expression<Func<SetPropertyCalls<Book>, SetPropertyCalls<Book>>>>(), 
                                                        It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.Update(It.IsAny<BookBorrowingRequest>()));
        _mockRequestRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<EmailRequest>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateStatus(requestId, new UpdateStatusRequest { Status = RequestStatus.Rejected });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _mockBookRepo.Verify(x => x.ExecuteUpdateAsync(It.IsAny<Expression<Func<Book, bool>>>(), It.IsAny<Expression<Func<SetPropertyCalls<Book>, SetPropertyCalls<Book>>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_Approved_UpdatesBorrowingStatus()
    {
        var requestId = Guid.NewGuid();
        var borrowingRequest = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Waiting
        };

        _mockRequestRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync(borrowingRequest);
        _mockDetailRepo.Setup(x => x.ExecuteUpdateAsync(It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(), It.IsAny<Expression<Func<SetPropertyCalls<BookBorrowingRequestDetails>, SetPropertyCalls<BookBorrowingRequestDetails>>>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.Update(It.IsAny<BookBorrowingRequest>()));
        _mockRequestRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        _mockEmailService.Setup(s => s.GetTemplateFile(It.IsAny<string>())).ReturnsAsync("");
        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<EmailRequest>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateStatus(requestId, new UpdateStatusRequest { Status = RequestStatus.Approved });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _mockDetailRepo.Verify(x => x.ExecuteUpdateAsync(It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(), It.IsAny<Expression<Func<SetPropertyCalls<BookBorrowingRequestDetails>, SetPropertyCalls<BookBorrowingRequestDetails>>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ExceptionThrown_PerformsRollback()
    {
        var requestId = Guid.NewGuid();
        var borrowingRequest = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Waiting
        };

        _mockRequestRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync(borrowingRequest);
        _mockRequestRepo.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.Update(It.IsAny<BookBorrowingRequest>()));
        _mockRequestRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
        _mockRequestRepo.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        var result = await _service.UpdateStatus(requestId, new UpdateStatusRequest { Status = RequestStatus.Approved });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(ErrorMessage.ServerError());

        _mockRequestRepo.Verify(x => x.RollbackAsync(), Times.Once);
    }
}
