using Librow.Core.Enums;
using Librow.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Core.Entities;
public class BookBorrowingRequestDetails : Entity
{
    public Guid RequestId { get; set; }
    public Guid BookId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime ExtendedDueDate { get; set; }
    public BorrowingStatus Status { get; set; } // for updating when user returned book
    public BookBorrowingRequest BookBorrowingRequest { get; set; }
    public Book Book { get; set; }

}
