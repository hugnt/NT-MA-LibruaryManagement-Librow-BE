using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;

namespace Librow.Application.Services;
public interface IBookRatingService
{
    public Task<Result> GetByBookId(Guid bookId);
    public Task<Result> GetUserRight(Guid bookId);
    public Task<Result> Add(BookRatingRequest rating);
    
}
