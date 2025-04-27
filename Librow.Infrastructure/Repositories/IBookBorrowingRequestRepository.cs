using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Repositories;
public interface IBookBorrowingRequestRepository : IRepository<BookBorrowingRequest>
{
    public Task<BookBorrowingRequest?> GetDetailAsync(Expression<Func<BookBorrowingRequest, bool>> predicate, CancellationToken token = default);
}
