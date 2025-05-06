using Librow.Core.Entities;
using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Tests.MockSetup;
public class MockBookBorrowingRequestRepositorySetup
{

    public static List<BookBorrowingRequest> ListBorrowingRequests()
    {
        var res = new List<BookBorrowingRequest>()
        {
            new BookBorrowingRequest
            {
                Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                RequestorId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"), // user001
                ApproverId = null, // admin
                Status = RequestStatus.Waiting,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            },
            new BookBorrowingRequest
            {
                Id = Guid.Parse("e2222222-2222-2222-2222-222222222222"),
                RequestorId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c48"), // user002
                ApproverId = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"), // admin
                Status = RequestStatus.Approved,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            },
            new BookBorrowingRequest
            {
                Id = Guid.Parse("e3333333-3333-3333-3333-333333333333"),
                RequestorId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c45"), // user003
                ApproverId = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"), // admin
                Status = RequestStatus.Rejected,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            },
            new BookBorrowingRequest
            {
                Id = Guid.Parse("e4444444-4444-4444-4444-444444444444"),
                RequestorId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c48"), // user002
                ApproverId = null, // admin
                Status = RequestStatus.Waiting,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            },
            new BookBorrowingRequest
            {
                Id = Guid.Parse("e5555555-5555-5555-5555-555555555555"),
                RequestorId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c48"), // user002
                ApproverId = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"), // admin
                Status = RequestStatus.Approved,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            },
            new BookBorrowingRequest
            {
                Id = Guid.Parse("e6666666-6666-6666-6666-666666666666"),
                RequestorId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c45"), // user003
                ApproverId = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"), // admin
                Status = RequestStatus.Rejected,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            },
        };

        foreach (var request in res)
        {
            request.Approver = MockUserRepositorySetup.ListUsers().FirstOrDefault(x => x.Id == request.ApproverId);
            request.Requestor = MockUserRepositorySetup.ListUsers().FirstOrDefault(x => x.Id == request.RequestorId)!;
            var listDetails = MockBookBorrowingRequestDetailsRepositorySetup.ListBookBorrowingRequestDetails(false).Where(x => x.RequestId == request.Id).ToList();
            if (listDetails != null)
            {
                request.BookBorrowingRequestDetails = listDetails;
            }
           
        }
        return res;
    }


}
