using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;

namespace Librow.Application.Services;
public interface IBookService
{
    public Task<Result> GetAll(BookFilterRequest filter);
    public Task<Result> GetById(Guid id);
    public Task<Result> Add(BookRequest newBook);
    public Task<Result> Update(Guid id, BookRequest updatedBook);
    public Task<Result> Delete(Guid id);
}
