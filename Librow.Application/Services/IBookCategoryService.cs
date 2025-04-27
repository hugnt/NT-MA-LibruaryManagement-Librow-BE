using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;

namespace Librow.Application.Services;
public interface IBookCategoryService
{
    public Task<Result> GetAll(FilterRequest filter);
    public Task<Result> GetById(Guid id);
    public Task<Result> Add(BookCategoryRequest newBookCategory);
    public Task<Result> Update(Guid id, BookCategoryRequest updatedBookCategory);
    public Task<Result> Delete(Guid id);
}
