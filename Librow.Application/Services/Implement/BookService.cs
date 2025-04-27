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
using System.Net;

namespace Librow.Application.Services.Implement;
public class BookService : IBookService
{
    private readonly IRepository<Book> _bookRepository;
    private readonly IRepository<BookCategory> _bookCategoryRepository; // IBookCategoryRepository
    private readonly IRepository<BookBorrowingRequestDetails> _bookBorrowingRequestDetailsRepository;
    private readonly IValidator<BookRequest> _bookValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BookService(IValidator<BookRequest> validator, IRepository<Book> bookRepository, 
                        IRepository<BookCategory> bookCategoryRepository, IHttpContextAccessor httpContextAccessor, 
                        IRepository<BookBorrowingRequestDetails> bookBorrowingRequestDetailsRepository)
    {
        _bookValidator = validator;
        _bookRepository = bookRepository;
        _bookCategoryRepository = bookCategoryRepository;
        _httpContextAccessor = httpContextAccessor;
        _bookBorrowingRequestDetailsRepository = bookBorrowingRequestDetailsRepository;
    }

    public async Task<Result> GetAll(FilterRequest filter)
    {
        var res = await _bookRepository.GetByFilterAsync(
            pageSize: filter.PageSize, 
            pageNumber: filter.PageNumber, 
            predicate:x => !x.IsDeleted, 
            selectQuery: x=>x.ToResponse(),
            navigationProperties: [x => x.BookCategory]
        );
        return FilterResult<List<BookResponse>>.Success(res.Data.ToList(), res.TotalCount);
    }
    public async Task<Result> GetById(Guid id)
    {
        var selectedEntity = await _bookRepository.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, navigationProperties: [x => x.BookCategory]);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Book "));
        }
        return Result<BookResponse>.SuccessWithBody(selectedEntity.ToResponse());
    }

    public async Task<Result> Add(BookRequest newBook)
    {
        var validateResult = _bookValidator.Validate(newBook);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        if(!await _bookCategoryRepository.AnyAsync(x => x.Id == newBook.CategoryId))
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(newBook.CategoryId, "Book Category"));
        }
        var bookEntity = newBook.ToEntity();
        bookEntity.CreatedAt = bookEntity.UpdatedAt = DateTime.Now;
        bookEntity.CreatedBy = bookEntity.UpdatedBy = ClaimHelper.GetItem<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
        bookEntity.Available = newBook.Quantity;

        _bookRepository.Add(bookEntity);
        await _bookRepository.SaveChangesAsync();
        return Result.SuccessWithMessage(SuccessMessage.CreatedSuccessfully("Book"));
    }

    public async Task<Result> Update(Guid id, BookRequest updatedBook)
    {
        var selectedEntity = await _bookRepository.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Book"));
        }

        var validateResult = _bookValidator.Validate(updatedBook);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }
        if (updatedBook.Quantity < selectedEntity.Available)
        {
            return Result.Error(HttpStatusCode.BadRequest, BookMessage.QuantityCanNotBeLowerThanAvailable);
        }
        if (!await _bookCategoryRepository.AnyAsync(x => x.Id == updatedBook.CategoryId))
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(updatedBook.CategoryId, "Book Category"));
        }

        if (selectedEntity.Quantity == 0)
        {
            selectedEntity.Available = updatedBook.Quantity;
        }
        selectedEntity.MappingFieldFrom(updatedBook);
        
        selectedEntity.UpdatedAt = DateTime.Now;
        selectedEntity.UpdatedBy = ClaimHelper.GetItem<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);

        _bookRepository.Update(selectedEntity);
        await _bookRepository.SaveChangesAsync();

        return Result.SuccessNoContent();
    }

    public async Task<Result> Delete(Guid id)
    {
        var selectedEntity = await _bookRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Book"));
        }
        if(selectedEntity.Quantity != selectedEntity.Available)
        {
            return Result.Error(HttpStatusCode.NotFound, BookMessage.BookExistedInOtherProcess);
        }
        if (await _bookBorrowingRequestDetailsRepository.AnyAsync(x => x.BookId == selectedEntity.Id))
        {
            selectedEntity.UpdatedAt = DateTime.Now;
            selectedEntity.UpdatedBy = ClaimHelper.GetItem<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);
            selectedEntity.IsDeleted = true;
            _bookRepository.Update(selectedEntity);
        }
        else
        {
            _bookRepository.Delete(selectedEntity);
        }

        await _bookRepository.SaveChangesAsync();

        return Result.SuccessNoContent();
    }


}
