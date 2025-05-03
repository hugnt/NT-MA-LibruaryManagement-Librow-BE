using Librow.Application.Helpers;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Mappings;
public static class BookBorrowingRequestMapping
{
    public static BorrowingRequestDetailsResponse ToDetailsResponse(this BookBorrowingRequest bookBorrowRequest) => new()
    {
        Id = bookBorrowRequest.Id,
        RequestorName = bookBorrowRequest.Requestor.Fullname,
        ApproverName = bookBorrowRequest?.Approver?.Fullname??"No approver yet",
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

    public static Expression<Func<BookBorrowingRequest, BorrowingRequestResponse>> SelectResponseExpression = x => new BorrowingRequestResponse
    {
        Id = x.Id,
        RequestorName = x.Requestor.Fullname,
        ApproverName = x.Approver == null ? "No approver" : x.Approver.Fullname,
        Status = x.Status,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

}

public class BookBorrowingRequestDetailsMapping
{

    public static Expression<Func<BookBorrowingRequestDetails, BorrowingBookResponse>> SelectResponseExpression = x => new BorrowingBookResponse
    {
        RequestId = x.RequestId,
        RequestDetailsId = x.Id,
        BookId = x.BookId,
        BookStatus = x.Status,
        RequestStatus = x.BookBorrowingRequest.Status,
        BookName = x.Book.Title,
        Author = x.Book.Author,
        DueDate = x.DueDate,
        ExtendedDueDate = x.ExtendedDueDate,
    };
}

