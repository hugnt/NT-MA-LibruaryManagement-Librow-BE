using Librow.Application.Common.Email;
using Librow.Application.Models.Requests;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using Moq;
using System.Linq.Expressions;

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;

public class CheckOverdueBooksTests
{
    private readonly Mock<IRepository<BookBorrowingRequestDetails>> _mockDetailRepo;
    private readonly Mock<BookBorrowingRequestService> _mockService;
    private readonly Mock<IEmailService> _mockEmailService;

    public CheckOverdueBooksTests()
    {
        _mockDetailRepo = new Mock<IRepository<BookBorrowingRequestDetails>>();
        _mockEmailService = new Mock<IEmailService>();

        // Create a partial mock to override SendMailForOverdueBorrowingBooks
        _mockService = new Mock<BookBorrowingRequestService>(null!, null!, _mockDetailRepo.Object, null!, _mockEmailService.Object, null!);
    }

    [Fact]
    public async Task CheckOverdueBooks_NoOverdueRequests_DoesNotSendMail()
    {
        _mockDetailRepo.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(),
                It.IsAny<Expression<Func<BookBorrowingRequestDetails, OverdueRequest>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<BookBorrowingRequestDetails, object>>[]>()
                )).ReturnsAsync(new List<OverdueRequest>());

        await _mockService.Object.CheckOverdueBooks();

        _mockEmailService.Verify(s => s.SendEmailAsync(It.IsAny<EmailRequest>()), Times.Never);
    }

    [Fact]
    public async Task CheckOverdueBooks_HasOverdueRequests_SendsMailForEachUser()
    {
        var today = DateTime.Today;
        var list = new List<OverdueRequest>
        {
            new()
            {
                RequestorId = Guid.NewGuid(),
                RequestorName = "Alice",
                RequestorEmail = "alice@example.com",
                BookName = "Book A",
                ExtendedDueDate = today.AddDays(-3)
            },
            new()
            {
                RequestorId = Guid.NewGuid(),
                RequestorName = "Bob",
                RequestorEmail = "bob@example.com",
                BookName = "Book B",
                ExtendedDueDate = today.AddDays(-1)
            },
            new()
            {
                RequestorId = Guid.NewGuid(),
                RequestorName = "Alice",
                RequestorEmail = "alice@example.com",
                BookName = "Book C",
                ExtendedDueDate = today.AddDays(-5)
            }
        };

        _mockDetailRepo.Setup(r => r.GetAllAsync(
                 It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(),
                It.IsAny<Expression<Func<BookBorrowingRequestDetails, OverdueRequest>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<BookBorrowingRequestDetails, object>>[]>()
            )).ReturnsAsync(list);

        _mockEmailService.Setup(s => s.GetTemplateFile(It.IsAny<string>())).ReturnsAsync("");
        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<EmailRequest>())).Returns(Task.CompletedTask);

        await _mockService.Object.CheckOverdueBooks();

        _mockEmailService.Verify(s => s.SendEmailAsync(It.IsAny<EmailRequest>()), Times.Exactly(3));
    }
}
