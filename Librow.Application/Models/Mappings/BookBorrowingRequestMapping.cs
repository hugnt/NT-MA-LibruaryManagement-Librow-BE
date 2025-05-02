using Librow.Application.Helpers;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Mappings;
public static class BookBorrowingRequestMapping
{
    public static BorrowingRequestResponse ToResponse(this BookBorrowingRequest bookBorrowRequest) => new()
    {
        Id = bookBorrowRequest.Id,
        RequestorName = bookBorrowRequest.Requestor.Fullname,
        ApproverName = bookBorrowRequest.Approver?.Fullname??"No approver",
        StatusName = StatusHelper.GetStatusName(bookBorrowRequest.Status),
        Status = bookBorrowRequest.Status,
        CreatedAt = bookBorrowRequest.CreatedAt,
        UpdatedAt = bookBorrowRequest.UpdatedAt,
    };

    public static BorrowingRequestDetailsResponse ToDetailsResponse(this BookBorrowingRequest bookBorrowRequest) => new()
    {
        Id = bookBorrowRequest.Id,
        RequestorName = bookBorrowRequest.Requestor.Fullname,
        ApproverName = bookBorrowRequest?.Approver?.Fullname??"No approver yet",
        StatusName = StatusHelper.GetStatusName(bookBorrowRequest.Status),
        Status = bookBorrowRequest.Status,
        CreatedAt = bookBorrowRequest.CreatedAt,
        UpdatedAt = bookBorrowRequest.UpdatedAt,
        Details = bookBorrowRequest.BookBorrowingRequestDetails.Select(x => new BorrowingDetailsResponse()
        {
            Id = x.Id,
            BookId = x.BookId,
            BookName = x.Book.Title,
            Author = x.Book.Author,
            DueDate = x.DueDate,  
            ExtendedDueDate = x.ExtendedDueDate
        }).ToList()
    };

    public static BorrowingBookResponse ToResponse(this BookBorrowingRequestDetails bookBorrowRequestDetails) => new()
    {
        RequestId = bookBorrowRequestDetails.RequestId,
        RequestDetailsId = bookBorrowRequestDetails.Id,
        BookId = bookBorrowRequestDetails.BookId,
        Status = bookBorrowRequestDetails.Status,
        BookName = bookBorrowRequestDetails.Book.Title,
        Author = bookBorrowRequestDetails.Book.Author,
        DueDate = bookBorrowRequestDetails.DueDate,
        ExtendedDueDate = bookBorrowRequestDetails.ExtendedDueDate,
    };


}

