using FluentAssertions;
using Librow.Application.Common.Email;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security.Token;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;
public class AddTests
{
    private readonly Mock<IBookBorrowingRequestRepository> _mockRequestRepo;
    private readonly Mock<IRepository<BookBorrowingRequestDetails>> _mockDetailRepo;
    private readonly Mock<IRepository<Book>> _mockBookRepo;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContext;
    private readonly BookBorrowingRequestService _service;
    private readonly Guid _userId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d");

    public AddTests()
    {
        _mockRequestRepo = new Mock<IBookBorrowingRequestRepository>();
        _mockDetailRepo = new Mock<IRepository<BookBorrowingRequestDetails>>();
        _mockBookRepo = new Mock<IRepository<Book>>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockEmailService = new Mock<IEmailService>();
        _mockHttpContext = new Mock<IHttpContextAccessor>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimType.Id, _userId.ToString()),
            new Claim(ClaimType.Role, Role.Customer.ToString())
        }));
        _mockHttpContext.Setup(x => x.HttpContext!.User).Returns(user);

        _service = new BookBorrowingRequestService(_mockBookRepo.Object, _mockHttpContext.Object,_mockDetailRepo.Object, _mockRequestRepo.Object, _mockEmailService.Object, _mockUserRepo.Object);
    }

    [Fact]
    public async Task Add_NullDetails_ReturnsBadRequest()
    {
        var request = new BorrowingRequest { Details = null };

        var result = await _service.Add(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(ErrorMessage.ObjectCanNotBeNullOrEmpty("Request"));
    }

    [Fact]
    public async Task Add_OverLimitRequestsInMonth_ReturnsBadRequest()
    {
        var request = new BorrowingRequest
        {
            Details = [new() { BookId = Guid.NewGuid(), DueDate = DateTime.Now.AddDays(10) }]
        };

        _mockRequestRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync(3);

        var result = await _service.Add(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(BorrowingRequestMessage.ErrorOverLimitedRequest(3));
    }

    [Fact]
    public async Task Add_OverLimitedBooksInRequest_ReturnsBadRequest()
    {
        var details = Enumerable.Range(0, 6)
            .Select(i => new BorrowingDetailsRequest { BookId = Guid.NewGuid(), DueDate = DateTime.Now.AddDays(5) })
            .ToList();

        var request = new BorrowingRequest { Details = details };

        _mockRequestRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync(0);

        var result = await _service.Add(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(BorrowingRequestMessage.ErrorOverLimitedBook(5));
    }

    [Fact]
    public async Task Add_InvalidDueDate_ReturnsBadRequest()
    {
        var request = new BorrowingRequest
        {
            Details = [new() { BookId = Guid.NewGuid(), DueDate = DateTime.Now.AddDays(-1) }]
        };

        _mockRequestRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default))
            .ReturnsAsync(0);

        var result = await _service.Add(request);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Add_UnavailableBooks_ReturnsNotFound()
    {
        var bookId = Guid.NewGuid();
        var request = new BorrowingRequest
        {
            Details = [new() { BookId = bookId, DueDate = DateTime.Now.AddDays(5) }]
        };

        _mockRequestRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default))
            .ReturnsAsync(0);
        _mockBookRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Book, bool>>>(), default))
            .ReturnsAsync(0); // Book not available

        var result = await _service.Add(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Add_ValidRequest_ReturnsCreated()
    {
        var bookId = MockBookRepositorySetup.ListBooks().First().Id;
        var dueDate = DateTime.Now.AddDays(5);
        var request = new BorrowingRequest
        {
            Details = [new() { BookId = bookId, DueDate = dueDate }]
        };

        _mockRequestRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync(0);
        _mockBookRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Book, bool>>>(), default)).ReturnsAsync(1);

        _mockRequestRepo.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.Add(It.IsAny<BookBorrowingRequest>()));
        _mockDetailRepo.Setup(x => x.AddRangeAsync(It.IsAny<List<BookBorrowingRequestDetails>>())).Returns(Task.CompletedTask);
        _mockBookRepo.Setup(repo => repo.ExecuteUpdateAsync(
                    It.IsAny<Expression<Func<Book, bool>>>(),
                    It.IsAny<Expression<Func<SetPropertyCalls<Book>, SetPropertyCalls<Book>>>>(),
                    It.IsAny<CancellationToken>()
                )).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _mockUserRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>?>(), 
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()
            ))
            .ReturnsAsync(MockUserRepositorySetup.ListUsers().First(x=>x.Id== _userId));

        _mockEmailService
           .Setup(s => s.GetTemplateFile(It.IsAny<string>()))
           .ReturnsAsync("");


        _mockEmailService
            .Setup(s => s.SendEmailAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);

        _mockRequestRepo.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
   
        var result = await _service.Add(request);

        _mockEmailService.Verify(s => s.SendEmailAsync(It.IsAny<EmailRequest>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Add_TwoRequestsWithAvailableOne_ReturnsConcurrencyException()
    {
        var bookId = MockBookRepositorySetup.ListBooks().First().Id;
        var dueDate = DateTime.Now.AddDays(5);
        var request1 = new BorrowingRequest
        {
            Details = [new() { BookId = bookId, DueDate = dueDate }]
        };

        var request2 = new BorrowingRequest
        {
            Details = [new() { BookId = bookId, DueDate = dueDate.AddDays(1) }]
        };

        var initialRowVersion = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
        var lstBooks = MockBookRepositorySetup.ListBooks();
        lstBooks[0].RowVersion = initialRowVersion;

        _mockRequestRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<BookBorrowingRequest, bool>>>(), default)).ReturnsAsync(0);
        _mockBookRepo.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Book, bool>>>(), default)).ReturnsAsync(1); // Only 1 book available

        _mockRequestRepo.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.Add(It.IsAny<BookBorrowingRequest>()));
        _mockDetailRepo.Setup(x => x.AddRangeAsync(It.IsAny<List<BookBorrowingRequestDetails>>())).Returns(Task.CompletedTask);

        // Mock ExecuteUpdateAsync to simulate concurrency using SetupSequence
     
        _mockBookRepo.Setup(r => r.ExecuteUpdateAsync(
                It.IsAny<Expression<Func<Book, bool>>>(),
                It.IsAny<Expression<Func<SetPropertyCalls<Book>, SetPropertyCalls<Book>>>>(),
                It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    // Update book set new rowversion = new where rowversion = old_rowvewrsion
                    var index = lstBooks.FindIndex(x => x.RowVersion == initialRowVersion);
                    if (index != -1)
                    {
                        lstBooks[index].RowVersion = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
                        return Task.CompletedTask;
                    }
                    throw new DbUpdateConcurrencyException("Concurrency conflict due to RowVersion mismatch");
                });

        _mockRequestRepo.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        _mockRequestRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));  

        _mockUserRepo
           .Setup(r => r.FirstOrDefaultAsync(
               It.IsAny<Expression<Func<User, bool>>?>(),
               It.IsAny<CancellationToken>(),
               It.IsAny<Expression<Func<User, object>>[]>()
           ))
           .ReturnsAsync(MockUserRepositorySetup.ListUsers().First(x => x.Id == _userId));

        _mockEmailService
           .Setup(s => s.GetTemplateFile(It.IsAny<string>()))
           .ReturnsAsync("");

        _mockEmailService
            .Setup(s => s.SendEmailAsync(It.IsAny<EmailRequest>()))
            .Returns(Task.CompletedTask);


        _mockRequestRepo.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        // Act
        var task1 = _service.Add(request1);
        var task2 = _service.Add(request2);
        var results = await Task.WhenAll(task1, task2);

        // Assert
        var successResult = results.FirstOrDefault(r => r.IsSuccess);
        var failureResult = results.FirstOrDefault(r => !r.IsSuccess);

        successResult.Should().NotBeNull();
        successResult.IsSuccess.Should().BeTrue();
        successResult.StatusCode.Should().Be(HttpStatusCode.Created);

        failureResult.Should().NotBeNull();
        failureResult.IsSuccess.Should().BeFalse();
        failureResult.StatusCode.Should().Be(HttpStatusCode.Conflict);
        failureResult.Errors.Should().Contain(ErrorMessage.ConcurrencyConlict);

        // Assert both tasks returned a concurrency conflict
        task1.Result.IsSuccess.Should().NotBe(task2.Result.IsSuccess);

        // Kiểm tra rằng chỉ có một yêu cầu thực hiện thành công
        _mockRequestRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRequestRepo.Verify(x => x.CommitAsync(), Times.Once);
        _mockEmailService.Verify(s => s.SendEmailAsync(It.IsAny<EmailRequest>()), Times.Once);
    }


}
