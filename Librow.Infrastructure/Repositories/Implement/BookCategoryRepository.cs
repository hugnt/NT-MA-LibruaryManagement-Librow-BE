using Librow.Core.Entities;
using Librow.Infrastructure.Common;
using Librow.Infrastructure.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Repositories.Implement;
public class BookCategoryRepository : Repository<BookCategory>, IBookCategoryRepository
{
    public BookCategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<BookCategory> GetDefaultCategory()
    {
        var id = SeedData.DefaultBookCategory.Id;
        return await _context.BookCategories.FindAsync(id) ?? throw new InvalidOperationException("Default book category not found.");
    }
}
