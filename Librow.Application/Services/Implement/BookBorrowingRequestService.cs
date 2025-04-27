using FluentValidation;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security.Token;
using Librow.Application.Helpers;
using Librow.Application.Models;
using Librow.Application.Models.Mappings;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Librow.Infrastructure.Repositories.Implement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Librow.Application.Services.Implement;
public class BookBorrowingRequestService : IBookBorrowingRequestService
{
    private readonly IRepository<Book> _bookRepository;
    private readonly IBookBorrowingRequestRepository _bookBorrowingRequestRepository;
    private readonly IRepository<BookBorrowingRequestDetails> _bookBorrowingRequestDetailsRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const int MAX_BOOK_PER_REQUEST = 5;
    private const int MAX_REQUEST_PER_MONTH = 3;

    public BookBorrowingRequestService(IRepository<Book> bookRepository, IHttpContextAccessor httpContextAccessor,
                                        IRepository<BookBorrowingRequestDetails> bookBorrowingRequestDetailsRepository,
                                        IBookBorrowingRequestRepository bookBorrowingRequestRepository)
    {
        _bookRepository = bookRepository;
        _httpContextAccessor = httpContextAccessor;
        _bookBorrowingRequestDetailsRepository = bookBorrowingRequestDetailsRepository;
        _bookBorrowingRequestRepository = bookBorrowingRequestRepository;
    }

    public async Task<Result> GetAll(BorrowingRequestFilter filter)
    {
        var res = await _bookBorrowingRequestRepository.GetByFilterAsync(
                                                            filter.PageSize,filter.PageNumber, 
                                                            predicate: x => x.Status == filter.Status,           
                                                            selectQuery: x => x.ToResponse());
        return FilterResult<List<BorrowingRequestResponse>>.Success(res.Data.ToList(), res.TotalCount);
    }

    public async Task<Result> GetById(Guid id)
    {
        var selectedEntity = await _bookBorrowingRequestRepository.GetDetailAsync(x => x.Id == id);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Borrowing Request "));
        }
        return Result<BorrowingRequestDetailsResponse>.SuccessWithBody(selectedEntity.ToDetailsResponse());
    }

    public async Task<Result> Add(BorrowingRequest newRequest)
    {
        if(newRequest.Details == null || newRequest.Details.Count == 0)
        {
            return Result.Error(HttpStatusCode.BadRequest, ErrorMessage.ObjectCanNotBeNullOrEmpty("Request "));
        }
        var requestorId = ClaimHelper.GetItem<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
        //check if reach limit (3) of request in month
        if (await _bookBorrowingRequestRepository.CountAsync(x => x.RequestorId == requestorId
                                                                && x.CreatedAt.Month == DateTime.Now.Month 
                                                                && x.CreatedAt.Year == DateTime.Now.Year) >= MAX_REQUEST_PER_MONTH)
        {
            return Result.Error(HttpStatusCode.BadRequest, BorrowingRequestMessage.ErrorOverLimitedRequest(MAX_REQUEST_PER_MONTH));
        }

        // Check if over 5 books
        if (newRequest.Details.Count > MAX_BOOK_PER_REQUEST)
        {
            return Result.Error(HttpStatusCode.BadRequest, BorrowingRequestMessage.ErrorOverLimitedBook(MAX_BOOK_PER_REQUEST));
        }

        // Check due date
        var invalidDetailDate = newRequest.Details.FirstOrDefault(x => x.DueDate < DateTime.Now);
        if (invalidDetailDate != null)
        {
            return Result.Error(HttpStatusCode.BadRequest, BorrowingRequestMessage.ErrorInvalidDueDate(invalidDetailDate.DueDate));
        }

        // Check book existed & avalable
        if (await _bookRepository.CountAsync(x => newRequest.Details.Select(b => b.BookId).Contains(x.Id) && x.Available > 0) == newRequest.Details.Count)
        {
            return Result.Error(HttpStatusCode.NotFound, BorrowingRequestMessage.ErrorBookIsNotAvailable);
        }

        try
        {
            await _bookBorrowingRequestRepository.BeginTransactionAsync();

            //Add new request
            var requestEntity = new BookBorrowingRequest();
            requestEntity.CreatedAt = requestEntity.UpdatedAt = DateTime.Now;
            requestEntity.RequestorId = requestEntity.CreatedBy = requestEntity.UpdatedBy = requestorId;
            requestEntity.Status = RequestStatus.Waiting;
            _bookBorrowingRequestRepository.Add(requestEntity);

            //Add request details
            var lstDetailsEntity = new List<BookBorrowingRequestDetails>();
            foreach (var detail in newRequest.Details)
            {
                lstDetailsEntity.Add(new BookBorrowingRequestDetails()
                {
                    RequestId = requestEntity.Id,
                    BookId = detail.BookId,
                    DueDate = detail.DueDate,
                    ExtendedDueDate = detail.DueDate,
                    Status = BorrowingStatus.Borrowing
                });
            }
            
            await _bookBorrowingRequestDetailsRepository.AddRangeAsync(lstDetailsEntity);

            // decrease number of available book
            await _bookRepository.ExecuteUpdateAsync(predicate: x => newRequest.Details.Select(b => b.BookId).Contains(x.Id),
                                                    updateExpression: setters => setters.SetProperty(b => b.Available, b => b.Available - 1));

            await _bookBorrowingRequestRepository.SaveChangesAsync();
            await _bookRepository.CommitAsync();

        }
        catch (DbUpdateConcurrencyException)
        {
            await _bookBorrowingRequestRepository.RollbackAsync();
            return Result.Error(HttpStatusCode.Conflict, ErrorMessage.ConcurrencyConlict);
        }
        catch (Exception)
        {
            await _bookBorrowingRequestRepository.RollbackAsync();
            return Result.Error(HttpStatusCode.InternalServerError, ErrorMessage.ServerError());
        }
      
        return Result.SuccessWithMessage(SuccessMessage.CreatedSuccessfully("Borrowing request"));
    }

    public async Task<Result> UpdateStatus(Guid id, UpdateStatusRequest updatedStatusRequest)
    {
        var selectedEntity = await _bookBorrowingRequestRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Borrowing Request "));
        }
        selectedEntity.Status = updatedStatusRequest.Status;
        selectedEntity.UpdatedAt = DateTime.Now;
        selectedEntity.ApproverId = selectedEntity.UpdatedBy = ClaimHelper.GetItem<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id); ;

        _bookBorrowingRequestRepository.Update(selectedEntity);
        await _bookBorrowingRequestRepository.SaveChangesAsync();
        return Result.SuccessNoContent();
    }

    public async Task<Result> UpdateExtendedDueDate(Guid id, ExtendBorrowingRequest extendBorrowingRequest)
    {
        var selectedEntity = await _bookBorrowingRequestDetailsRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (selectedEntity == null) 
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Borrowing Request of boo"));
        }
        if(selectedEntity.DueDate.Date < selectedEntity.ExtendedDueDate.Date)
        {
            return Result.Error(HttpStatusCode.BadRequest, BorrowingRequestMessage.ErrorOverLimitedExpandedDate);
        }
        if(selectedEntity.DueDate.Date >= extendBorrowingRequest.ExtendedDueDate.Date)
        {
            return Result.Error(HttpStatusCode.BadRequest, BorrowingRequestMessage.ErrorInvalidExpandedDueDate(selectedEntity.DueDate));
        }
        selectedEntity.ExtendedDueDate = extendBorrowingRequest.ExtendedDueDate;

        _bookBorrowingRequestDetailsRepository.Update(selectedEntity);
        await _bookBorrowingRequestDetailsRepository.SaveChangesAsync();
        return Result.SuccessNoContent();
    }

    public async Task<Result> UpdateBorrowingBookStatus(Guid id, UpdateBorrowingStatusRequest updateBorrowingStatus)
    {
        var selectedEntity = await _bookBorrowingRequestDetailsRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Borrowing Request "));
        }
        if(selectedEntity.Status == BorrowingStatus.Returned)
        {
            return Result.Error(HttpStatusCode.BadRequest, BorrowingRequestMessage.ErrorBookReturnCanNotUpdateToOtherStatus);
        }

        try
        {
            await _bookBorrowingRequestDetailsRepository.BeginTransactionAsync();
            if (updateBorrowingStatus.Status == BorrowingStatus.Returned)
            {
                var selectedBookEntity = await _bookRepository.FirstOrDefaultAsync(x => x.Id == selectedEntity.BookId);
                selectedBookEntity!.Available += 1;
                _bookRepository.Update(selectedBookEntity);
            }
            selectedEntity.Status = updateBorrowingStatus.Status;

            _bookBorrowingRequestDetailsRepository.Update(selectedEntity);
            await _bookBorrowingRequestDetailsRepository.SaveChangesAsync();
            await _bookBorrowingRequestDetailsRepository.CommitAsync();
        }
        catch (Exception)
        {
            await _bookBorrowingRequestDetailsRepository.RollbackAsync();
            return Result.Error(HttpStatusCode.InternalServerError, ErrorMessage.ServerError());
        }
       
        return Result.SuccessNoContent();
    }
}
