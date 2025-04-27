using Librow.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Core.Entities;
public class Book : Entity, IAuditableEntity, IConcurrencyEntity
{
    public string Title { get; set; }
    public Guid CategoryId { get; set; }
    public string Author { get; set; }
    public int Quantity { get; set; }
    public int Available { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public BookCategory BookCategory { get; set; }
    public IList<BookBorrowingRequestDetails> BookBorrowingRequestDetails { get; set; }
    public IList<BookRating> BookRatings { get; set; }
    public byte[] RowVersion { get; set; }
}
