using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Enums;

namespace Librow.Application.Services;
public interface IBookBorrowingRequestService
{
    public Task<Result> GetAll(BorrowingRequestFilter filter);
    public Task<Result> GetAllBorrowingBooks(FilterRequest filter);
    public Task<Result> GetUserRequestInfo(RequestFilter requestFilter);
    public Task<Result> GetById(Guid id);
    public Task<Result> Add(BorrowingRequest newBookBorrowingRequest);
    public Task<Result> UpdateStatus(Guid id, UpdateStatusRequest updatedStatusRequest);
    public Task<Result> UpdateExtendedDueDate(Guid id, ExtendBorrowingRequest extendBorrowingRequest);
    public Task<Result> UpdateBorrowingBookStatus(Guid id, UpdateBorrowingStatusRequest updateBorrowingStatus);
}
