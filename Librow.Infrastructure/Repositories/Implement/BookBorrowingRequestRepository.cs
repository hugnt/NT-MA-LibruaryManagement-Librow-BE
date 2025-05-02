using Librow.Core.Entities;
using Librow.Infrastructure.Common;
using Librow.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Repositories.Implement;
public class BookBorrowingRequestRepository : Repository<BookBorrowingRequest>, IBookBorrowingRequestRepository
{
    public BookBorrowingRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<BookBorrowingRequest?> GetDetailAsync(Expression<Func<BookBorrowingRequest, bool>> predicate, CancellationToken token = default)
    {
        return await _context.Set<BookBorrowingRequest>()
                                .Include(x=>x.Requestor)
                                .Include(x => x.Approver)
                                .Include(x => x.BookBorrowingRequestDetails)
                                .ThenInclude(y => y.Book)
                                .FirstOrDefaultAsync(predicate, token);
                        
    }
}
