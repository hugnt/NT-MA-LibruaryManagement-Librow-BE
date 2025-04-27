using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Repositories;
public interface IBookCategoryRepository : IRepository<BookCategory>
{
    public Task<BookCategory> GetDefaultCategory();
}
