using Librow.Core.Enums;
using Librow.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Core.Entities;
public class BookBorrowingRequest : Entity, IAuditableEntity
{
    public Guid RequestorId { get; set; }
    public Guid? ApproverId { get; set; }
    public RequestStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public User Requestor { get; set; }
    public User Approver { get; set; }
    public IList<BookBorrowingRequestDetails> BookBorrowingRequestDetails { get; set; }
}
