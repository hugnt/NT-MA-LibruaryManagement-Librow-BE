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
public class BookCategoryService : IBookCategoryService
{
    private readonly IBookCategoryRepository _bookCategoryRepository;
    private readonly IRepository<Book> _bookRepository;
    private readonly IValidator<BookCategoryRequest> _bookCategoryValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BookCategoryService(IBookCategoryRepository repository,
                               IValidator<BookCategoryRequest> validator,
                               IRepository<Book> bookRepository,
                               IHttpContextAccessor httpContextAccessor)
    {
        _bookCategoryRepository = repository;
        _bookCategoryValidator = validator;
        _bookRepository = bookRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> GetAll(FilterRequest filter)
    {
        var res = await _bookCategoryRepository.GetByFilterAsync(filter.PageSize, filter.PageNumber, selectQuery: BookCategoryMapping.SelectResponseExpression);
        return FilterResult<List<BookCategoryResponse>>.Success(res.Data.ToList(), res.TotalCount);
    }
    public async Task<Result> GetById(Guid id)
    {
        var selectedEntity = await _bookCategoryRepository.FirstOrDefaultAsync(x => x.Id == id, selectQuery: BookCategoryMapping.SelectResponseExpression);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Book Category"));
        }
        return Result<BookCategoryResponse>.SuccessWithBody(selectedEntity);
    }

    public async Task<Result> Add(BookCategoryRequest newBookCategory)
    {
        var validateResult = _bookCategoryValidator.Validate(newBookCategory);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        } 
        if(await _bookCategoryRepository.AnyAsync(x=> x.Name == newBookCategory.Name.Trim()))
        {
            return Result.Error(HttpStatusCode.BadRequest, ErrorMessage.ObjectExisted(newBookCategory.Name, "Book Category"));
        }

        var bookCategoryEntity = newBookCategory.ToEntity();

        bookCategoryEntity.CreatedAt = bookCategoryEntity.UpdatedAt = DateTime.Now;
        bookCategoryEntity.CreatedBy = bookCategoryEntity.UpdatedBy = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);

        _bookCategoryRepository.Add(bookCategoryEntity);
        await _bookCategoryRepository.SaveChangesAsync();
        return Result.SuccessWithMessage(SuccessMessage.CreatedSuccessfully("Book Category"));
    }

    public async Task<Result> Update(Guid id, BookCategoryRequest updatedBookCategory)
    {
        var selectedEntity = await _bookCategoryRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Book Category"));
        }

        var validateResult = _bookCategoryValidator.Validate(updatedBookCategory);
        if (!validateResult.IsValid)
        {
            return Result.ErrorValidation(validateResult);
        }

        selectedEntity.MappingFieldFrom(updatedBookCategory);

        selectedEntity.UpdatedAt = DateTime.Now;
        selectedEntity.UpdatedBy = ClaimHelper.GetClaimValue<Guid>(_httpContextAccessor.HttpContext, ClaimType.Id);

        _bookCategoryRepository.Update(selectedEntity);
        await _bookCategoryRepository.SaveChangesAsync();

        return Result.SuccessNoContent();
    }

    public async Task<Result> Delete(Guid id)
    {
        var selectedEntity = await _bookCategoryRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (selectedEntity == null)
        {
            return Result.Error(HttpStatusCode.NotFound, ErrorMessage.ObjectNotFound(id, "Book Category"));
        }
        var defaultCategory = await _bookCategoryRepository.GetDefaultCategory();
        if(id == defaultCategory.Id)
        {
            return Result.Error(HttpStatusCode.BadRequest, ErrorMessage.ObjectCanNotBeDeleted(defaultCategory.Name, "Book Category"));
        }
        try
        {
            await _bookRepository.BeginTransactionAsync();
            await _bookRepository.ExecuteUpdateAsync(x => x.CategoryId == selectedEntity.Id,
                                       b => b.SetProperty(book => book.CategoryId, defaultCategory.Id)
                                               .SetProperty(book => book.UpdatedAt, DateTime.Now));

            _bookCategoryRepository.Delete(selectedEntity);
            await _bookCategoryRepository.SaveChangesAsync();
            await _bookRepository.CommitAsync();
        }
        catch (Exception)
        {
            await _bookRepository.RollbackAsync();
            return Result.Error(HttpStatusCode.InternalServerError, ErrorMessage.ServerError());
        }
       
        return Result.SuccessNoContent();
    }


}
