using FluentAssertions;
using Librow.Application.Common.Messages;
using Librow.Application.Models.Requests;
using Librow.Application.Services.Implement;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace Librow.Application.Tests.Services.BookBorrowingRequestServiceTests;

public class UpdateExtendedDueDateTests
{
    private readonly Mock<IRepository<BookBorrowingRequestDetails>> _mockDetailRepo;
    private readonly BookBorrowingRequestService _service;

    public UpdateExtendedDueDateTests()
    {
        _mockDetailRepo = new Mock<IRepository<BookBorrowingRequestDetails>>();
        _service = new BookBorrowingRequestService(null!, null!, _mockDetailRepo.Object, null!, null!, null!);
    }

    [Fact]
    public async Task UpdateExtendedDueDate_RequestNotFound_ReturnsNotFound()
    {
        var detailId = Guid.NewGuid();
        _mockDetailRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(), default))
                       .ReturnsAsync((BookBorrowingRequestDetails)null!);

        var result = await _service.UpdateExtendedDueDate(detailId, new ExtendBorrowingRequest { ExtendedDueDate = DateTime.Today.AddDays(3) });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Errors.Should().Contain(ErrorMessage.ObjectNotFound(detailId, "Borrowing Request of book"));
    }

    [Fact]
    public async Task UpdateExtendedDueDate_ExtendedDateGreaterThanOriginalExtended_ReturnsOverLimitError()
    {
        var detailId = Guid.NewGuid();
        var dueDate = DateTime.Today.AddDays(7);
        var currentExtendedDueDate = DateTime.Today.AddDays(10);
        var newRequest = new ExtendBorrowingRequest { ExtendedDueDate = DateTime.Today.AddDays(15) };

        var detail = new BookBorrowingRequestDetails
        {
            Id = detailId,
            DueDate = dueDate,
            ExtendedDueDate = currentExtendedDueDate
        };

        _mockDetailRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(), default))
                       .ReturnsAsync(detail);

        var result = await _service.UpdateExtendedDueDate(detailId, newRequest);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(BorrowingRequestMessage.ErrorOverLimitedExpandedDate);
    }

    [Fact]
    public async Task UpdateExtendedDueDate_ExtendedDateEarlierThanDueDate_ReturnsInvalidDateError()
    {
        var detailId = Guid.NewGuid();
        var dueDate = DateTime.Today.AddDays(10);
        var newRequest = new ExtendBorrowingRequest { ExtendedDueDate = DateTime.Today.AddDays(5) };

        var detail = new BookBorrowingRequestDetails
        {
            Id = detailId,
            DueDate = dueDate,
            ExtendedDueDate = dueDate
        };

        _mockDetailRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(), default))
                       .ReturnsAsync(detail);

        var result = await _service.UpdateExtendedDueDate(detailId, newRequest);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(BorrowingRequestMessage.ErrorInvalidExpandedDueDate(dueDate));
    }

    [Fact]
    public async Task UpdateExtendedDueDate_ValidRequest_UpdatesAndReturnsNoContent()
    {
        var detailId = Guid.NewGuid();
        var dueDate = DateTime.Today.AddDays(5);
        var newExtendedDate = DateTime.Today.AddDays(6);

        var detail = new BookBorrowingRequestDetails
        {
            Id = detailId,
            DueDate = dueDate,
            ExtendedDueDate = dueDate
        };

        _mockDetailRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<BookBorrowingRequestDetails, bool>>>(), default))
                       .ReturnsAsync(detail);
        _mockDetailRepo.Setup(r => r.Update(It.IsAny<BookBorrowingRequestDetails>()));
        _mockDetailRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateExtendedDueDate(detailId, new ExtendBorrowingRequest { ExtendedDueDate = newExtendedDate });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.ExtendedDueDate.Should().Be(newExtendedDate);
        _mockDetailRepo.Verify(x => x.Update(detail), Times.Once);
        _mockDetailRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
