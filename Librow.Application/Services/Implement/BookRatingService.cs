using FluentValidation;
using Librow.Application.Common.Messages;
using Librow.Application.Common.Security.Token;
using Librow.Application.Helpers;
using Librow.Application.Models;
using Librow.Application.Models.Mappings;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Net;

namespace Librow.Application.Services.Implement;
public class BookRatingService : IBookRatingService
{
    private readonly IRepository<Book> _bookRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<BookRating> _bookRatingRepository;
    private readonly IRepository<BookBorrowingRequest> _bookBorrowingRequestRepository;
    private readonly IValidator<BookRatingRequest> _ratingValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BookRatingService(IValidator<BookRatingRequest> validator, IRepository<Book> bookRepository,
                        IHttpContextAccessor httpContextAccessor,
                        IRepository<BookBorrowingRequest> bookBorrowingRequestRepository, IRepository<BookRating> bookRatingRepository, IRepository<User> userRepository)
    {
        _ratingValidator = validator;
        _bookRepository = bookRepository;
        _httpContextAccessor = httpContextAccessor;
        _bookBorrowingRequestRepository = bookBorrowingRequestRepository;
        _bookRatingRepository = bookRatingRepository;
        _userRepository = userRepository;
    }



    public async Task<Result> GetByBookId(Guid bookId)
    {
        if(!await _bookRepository.AnyAsync(x => x.Id == bookId))
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(bookId, "Book "));
        }
        var reviews= await _bookRatingRepository.GetAllAsync(predicate: x => x.BookId == bookId,
                                                          selectQuery: BookRatingMapping.SelectModelExpression,
                                                          navigationProperties: [x=> x.Reviewer]);

        var res = new BookRatingResponse()
        {
            AverageRating = reviews.Any() ? reviews.Average(x => x.Rate) : 0,
            Reviews = reviews.ToList()
        };
        return Result<BookRatingResponse>.SuccessWithBody(res);
    }

    public async Task<Result> GetUserRight(Guid bookId)
    {
        var checkUserRight = await CheckUserRight(bookId);
        return Result<bool>.SuccessWithBody(checkUserRight.IsValid);
    }

    public async Task<Result> Add(BookRatingRequest rating)
    {
        var validateResult = _ratingValidator.Validate(rating);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        var checkUserRight = await CheckUserRight(rating.BookId);
        if (!checkUserRight.IsValid) return Result.Error(checkUserRight.StatusCode, checkUserRight.Message);

        var bookRatingEntity = rating.ToEntity();
        bookRatingEntity.CreatedAt = bookRatingEntity.UpdatedAt = DateTime.Now;
        bookRatingEntity.ReviewerId = bookRatingEntity.CreatedBy = bookRatingEntity.UpdatedBy = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);

        _bookRatingRepository.Add(bookRatingEntity);
        await _bookRatingRepository.SaveChangesAsync();
        return Result.Success(HttpStatusCode.Created ,SuccessMessage.CreatedSuccessfully("Book"));
    }


    private async Task<(bool IsValid, HttpStatusCode StatusCode, string Message)> CheckUserRight(Guid bookId)
    {
        var userId = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
        if (!await _userRepository.AnyAsync(x => x.Id == userId && !x.IsDeleted))
        {
            return (false, HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(userId, "User"));
        }
        if (!await _bookRepository.AnyAsync(x => x.Id == bookId))
        {
            return (false, HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(bookId, "Book "));
        }
        var isUserHasRequestWithBook = await _bookBorrowingRequestRepository.AnyAsync(
            x => x.RequestorId == userId
            && x.BookBorrowingRequestDetails.Any(y => y.BookId == bookId)
        );

        return (isUserHasRequestWithBook, isUserHasRequestWithBook? HttpStatusCode.OK : HttpStatusCode.Forbidden, isUserHasRequestWithBook?"":ErrorMessage.UserHasNoPermission);
    }

}
