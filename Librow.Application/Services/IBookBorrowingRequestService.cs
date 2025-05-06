using Librow.Application.Models;
using Librow.Application.Models.Requests;

namespace Librow.Application.Services;
public interface IBookBorrowingRequestService
{
    public Task<Result> GetAll(BorrowingRequestFilter filter);
    public Task<Result> GetAllBorrowingBooks(BorrowingRequestFilter filter);
    public Task<Result> GetUserRequestInfo(RequestFilter requestFilter);
    public Task<Result> GetById(Guid id);
    public Task<Result> Add(BorrowingRequest newBookBorrowingRequest);
    public Task<Result> UpdateStatus(Guid id, UpdateStatusRequest updatedStatusRequest);
    public Task<Result> UpdateExtendedDueDate(Guid id, ExtendBorrowingRequest extendBorrowingRequest);
    public Task CheckOverdueBooks();
}
