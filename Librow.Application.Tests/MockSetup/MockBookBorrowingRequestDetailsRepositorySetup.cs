using Librow.Core.Entities;
using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Tests.MockSetup;
public class MockBookBorrowingRequestDetailsRepositorySetup
{
    public static List<BookBorrowingRequestDetails> ListBookBorrowingRequestDetails(bool includeBookBorrowingRequest = true)
    {
        var res = new List<BookBorrowingRequestDetails>()
        {
            new BookBorrowingRequestDetails
            {
                Id = Guid.Parse("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1"),
                RequestId = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                BookId = Guid.Parse("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1"),
                DueDate = DateTime.UtcNow.AddDays(7),
                ExtendedDueDate = DateTime.UtcNow.AddDays(14),
                Status = BorrowingStatus.None
            },
            new BookBorrowingRequestDetails
            {
                Id = Guid.Parse("e2e2e2e2-e2e2-e2e2-e2e2-e2e2e2e2e2e2"),
                RequestId = Guid.Parse("e2222222-2222-2222-2222-222222222222"),
                BookId = Guid.Parse("e2e2e2e2-e2e2-e2e2-e2e2-e2e2e2e2e2e2"),
                DueDate = DateTime.UtcNow.AddDays(10),
                ExtendedDueDate = DateTime.UtcNow.AddDays(17),
                Status = BorrowingStatus.Borrowing
            },
            new BookBorrowingRequestDetails
            {
                Id = Guid.Parse("f3f3f3f3-f3f3-f3f3-f3f3-f3f3f3f3f3f3"),
                RequestId = Guid.Parse("e3333333-3333-3333-3333-333333333333"),
                BookId = Guid.Parse("f3f3f3f3-f3f3-f3f3-f3f3-f3f3f3f3f3f3"),
                DueDate = DateTime.UtcNow.AddDays(5),
                ExtendedDueDate = DateTime.UtcNow.AddDays(12),
                Status = BorrowingStatus.None
            },
            new BookBorrowingRequestDetails
            {
                Id = Guid.Parse("a4a4a4a4-a4a4-a4a4-a4a4-a4a4a4a4a4a4"),
                RequestId = Guid.Parse("e4444444-4444-4444-4444-444444444444"),
                BookId = Guid.Parse("a4a4a4a4-a4a4-a4a4-a4a4-a4a4a4a4a4a4"),
                DueDate = DateTime.UtcNow.AddDays(3),
                ExtendedDueDate = DateTime.UtcNow.AddDays(10),
                Status = BorrowingStatus.None
            },
            new BookBorrowingRequestDetails
            {
                Id = Guid.Parse("b5b5b5b5-b5b5-b5b5-b5b5-b5b5b5b5b5b5"),
                RequestId = Guid.Parse("e5555555-5555-5555-5555-555555555555"),
                BookId = Guid.Parse("b5b5b5b5-b5b5-b5b5-b5b5-b5b5b5b5b5b5"),
                DueDate = DateTime.UtcNow.AddDays(6),
                ExtendedDueDate = DateTime.UtcNow.AddDays(13),
                Status = BorrowingStatus.Borrowing
            },
            new BookBorrowingRequestDetails
            {
                Id = Guid.Parse("c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6"),
                RequestId = Guid.Parse("e6666666-6666-6666-6666-666666666666"),
                BookId = Guid.Parse("c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6"),
                DueDate = DateTime.UtcNow.AddDays(8),
                ExtendedDueDate = DateTime.UtcNow.AddDays(15),
                Status = BorrowingStatus.None
            }
        };

        foreach (var detail in res)
        {
            detail.Book = MockBookRepositorySetup.ListBooks().First(x => x.Id == detail.BookId);
            if(includeBookBorrowingRequest) detail.BookBorrowingRequest = MockBookBorrowingRequestRepositorySetup.ListBorrowingRequests().FirstOrDefault(x => x.Id == detail.RequestId)!;
        }
        return res;
    }


}
