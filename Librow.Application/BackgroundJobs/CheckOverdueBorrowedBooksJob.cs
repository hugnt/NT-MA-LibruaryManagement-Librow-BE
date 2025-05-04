using Coravel.Invocable;
using Librow.Application.Services;

namespace Librow.Application.BackgroundJobs;

public class CheckOverdueBorrowedBooksJob : IInvocable
{
    private readonly IBookBorrowingRequestService _bookBorrowingRequestService;
    public CheckOverdueBorrowedBooksJob(IBookBorrowingRequestService bookBorrowingRequestService)
    {
        _bookBorrowingRequestService = bookBorrowingRequestService;
    }
    public async Task Invoke()
    {
        await _bookBorrowingRequestService.CheckOverdueBooks();
    }
}
