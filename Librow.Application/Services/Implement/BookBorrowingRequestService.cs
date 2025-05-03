using FluentValidation;
using Librow.Application.Common.Email;
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
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;

namespace Librow.Application.Services.Implement;
public class BookBorrowingRequestService : IBookBorrowingRequestService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Book> _bookRepository;
    private readonly IBookBorrowingRequestRepository _bookBorrowingRequestRepository;
    private readonly IRepository<BookBorrowingRequestDetails> _bookBorrowingRequestDetailsRepository;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const int MAX_BOOK_PER_REQUEST = 5;
    private const int MAX_REQUEST_PER_MONTH = 3;

    public BookBorrowingRequestService(IRepository<Book> bookRepository, IHttpContextAccessor httpContextAccessor,
                                        IRepository<BookBorrowingRequestDetails> bookBorrowingRequestDetailsRepository,
                                        IBookBorrowingRequestRepository bookBorrowingRequestRepository, IEmailService emailService, IRepository<User> userRepository)
    {
        _bookRepository = bookRepository;
        _httpContextAccessor = httpContextAccessor;
        _bookBorrowingRequestDetailsRepository = bookBorrowingRequestDetailsRepository;
        _bookBorrowingRequestRepository = bookBorrowingRequestRepository;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task<Result> GetAll(BorrowingRequestFilter filter)
    {
        var context = _httpContextAccessor.HttpContext;
        var role = ClaimHelper.GetClaimValue<Role>(context, ClaimType.Role);
        var userId = ClaimHelper.GetClaimValue<Guid>(context, ClaimType.Id);

        Expression<Func<BookBorrowingRequest, bool>> predicate = role == Role.Admin
                                                            ? x => x.Status == filter.Status
                                                            : x => x.Status == filter.Status && x.RequestorId == userId;

        var res = await _bookBorrowingRequestRepository.GetByFilterAsync(
            filter.PageSize,
            filter.PageNumber,
            predicate: predicate,
            selectQuery: BookBorrowingRequestMapping.SelectResponseExpression,
            navigationProperties: [x => x.Requestor, x => x.Approver]
        );

        return FilterResult<List<BorrowingRequestResponse>>.Success(res.Data.ToList(), res.TotalCount);
    }

    public async Task<Result> GetAllBorrowingBooks(BorrowingRequestFilter filter)
    {
        var context = _httpContextAccessor.HttpContext;
        var role = ClaimHelper.GetClaimValue<Role>(context, ClaimType.Role);
        var userId = ClaimHelper.GetClaimValue<Guid>(context, ClaimType.Id);

        Expression<Func<BookBorrowingRequestDetails, bool>> predicate = x =>
                                        (role == Role.Admin || x.BookBorrowingRequest.RequestorId == userId) &&
                                        (filter.Status == null || x.BookBorrowingRequest.Status == filter.Status);

        var res = await _bookBorrowingRequestDetailsRepository.GetByFilterAsync(
            filter.PageSize,
            filter.PageNumber,
            predicate: predicate,
            selectQuery: BookBorrowingRequestDetailsMapping.SelectResponseExpression,
            navigationProperties: [x => x.Book, x => x.BookBorrowingRequest]
        );

        return FilterResult<List<BorrowingBookResponse>>.Success(res.Data.ToList(), res.TotalCount);
    }

    public async Task<Result> GetUserRequestInfo(RequestFilter requestFilter)
    {
        var context = _httpContextAccessor.HttpContext;
        var userId = ClaimHelper.GetClaimValue<Guid>(context, ClaimType.Id);
        var totalRequest = await _bookBorrowingRequestRepository.CountAsync(x => x.RequestorId == userId &&
                                                                             x.CreatedAt >= requestFilter.StartDate &&
                                                                             x.CreatedAt <= requestFilter.EndDate);
        return Result<RequestFilterResponse>.SuccessWithBody(new RequestFilterResponse()
        {
            TotalRequest = totalRequest,
            MaxRequestPerMonth = MAX_REQUEST_PER_MONTH
        });
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
        var requestorId = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
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
        var listBookIds = newRequest.Details.Select(b => b.BookId);
        if (await _bookRepository.CountAsync(x => listBookIds.Contains(x.Id) && x.Available > 0) != newRequest.Details.Count)
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
                    Status = BorrowingStatus.None
                });
            }
            
            await _bookBorrowingRequestDetailsRepository.AddRangeAsync(lstDetailsEntity);

            // decrease number of available book
            await _bookRepository.ExecuteUpdateAsync(predicate: x => newRequest.Details.Select(b => b.BookId).Contains(x.Id),
                                                    updateExpression: setters => setters.SetProperty(b => b.Available, b => b.Available - 1));

            await _bookBorrowingRequestRepository.SaveChangesAsync();
            await _bookBorrowingRequestRepository.CommitAsync();

            await SendMailWithBorrowingStatus(requestEntity);

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
        var selectedEntity = await _bookBorrowingRequestRepository.FirstOrDefaultAsync(x => x.Id == id && x.Status == RequestStatus.Waiting);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Borrowing Request "));
        }
        selectedEntity.Status = updatedStatusRequest.Status;
        selectedEntity.UpdatedAt = DateTime.Now;
        selectedEntity.ApproverId = selectedEntity.UpdatedBy = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id); ;

        try
        {
            await _bookBorrowingRequestRepository.BeginTransactionAsync();
            if (updatedStatusRequest.Status == RequestStatus.Rejected)
            {
                // increase number of available book
                var allBooksOfRequest = await _bookBorrowingRequestDetailsRepository.GetAllAsync(predicate: x => x.RequestId == id, selectQuery: x => x.BookId);
                await _bookRepository.ExecuteUpdateAsync(predicate: x => allBooksOfRequest.Contains(x.Id),
                                                        updateExpression: setters => setters.SetProperty(b => b.Available, b => b.Available + 1));
            }
            else if (updatedStatusRequest.Status == RequestStatus.Approved)
            {
                await _bookBorrowingRequestDetailsRepository.ExecuteUpdateAsync(predicate: x => x.RequestId == id,
                                                            updateExpression: setters => setters.SetProperty(b => b.Status, b => BorrowingStatus.Borrowing));
            }
            _bookBorrowingRequestRepository.Update(selectedEntity);
            await _bookBorrowingRequestRepository.SaveChangesAsync();
            await _bookBorrowingRequestRepository.CommitAsync();

            await SendMailWithBorrowingStatus(selectedEntity);
        }
        catch (Exception)
        {
            await _bookBorrowingRequestRepository.RollbackAsync();
            return Result.Error(HttpStatusCode.InternalServerError, ErrorMessage.ServerError());
        }
      

        return Result.SuccessNoContent();
    }

    public async Task<Result> UpdateExtendedDueDate(Guid id, ExtendBorrowingRequest extendBorrowingRequest)
    {
        var selectedEntity = await _bookBorrowingRequestDetailsRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (selectedEntity == null) 
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Borrowing Request of book"));
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

    
    private async Task SendMailWithBorrowingStatus(BookBorrowingRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == request.RequestorId);
        if (user == null) return;
        try
        {
            var htmlEmail = await FileHelper.GetTemplateFile("NotificationOfStatusChanged.cshtml");
            htmlEmail = htmlEmail
                            .Replace("{{param_fullname}}", user.Fullname)
                            .Replace("{{param_status}}", EnumHelper.GetStatusName(request.Status))
                            .Replace("{{param_borrow_datetime}}", request.CreatedAt.ToString("MM/dd/yyyy"))
                           
                            .Replace("{{param_action_link}}", "http://localhost:5173/book-borrowing-request")
                            .Replace("{{param_fullname}}", user.Fullname)
                            .Replace("{{param_company_name}}", "Librow")
                            .Replace("{{param_company_phonenumber}}", "0999999999")
                            .Replace("{{param_company_address}}", "Hà Nội, Việt Nam")
                            .Replace("{{param_company_url}}", "hugnt.space");
            
            if (request.Status == RequestStatus.Approved || request.Status == RequestStatus.Rejected)
            {
                var userConfirm = await _userRepository.FirstOrDefaultAsync(x => x.Id == request.ApproverId);
                var confirmHtml = $@"
                    <li><strong>Confirmed On:</strong> {request.UpdatedAt.ToString("MM/dd/yyyy")}</li>
                    <li><strong>Confirmed By:</strong> {userConfirm?.Fullname??"Admin"}</li>
                ";
                htmlEmail = htmlEmail.Replace("{{param_confirm_info}}", confirmHtml);
            }
            else
            {
                htmlEmail = htmlEmail.Replace("{{param_confirm_info}}", "");
            }
            var emailRequest = new EmailRequest()
            {
                ToEmail = user.Email,
                Subject = "[Librow] Notification about status changing",
                Body = htmlEmail,
            };
            await _emailService.SendEmailAsync(emailRequest);

        }
        catch (Exception)
        {
            return;
        }
       

    }

}
