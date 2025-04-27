using Librow.Core.Enums;
using Librow.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Core.Entities;

public class User : Entity, IAuditableEntity
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; }
    public string Fullname { get; set; }
    public string Email { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public IList<BookBorrowingRequest> BookBorrowingRequestsAsRequestor { get; set; } = new List<BookBorrowingRequest>();
    public IList<BookBorrowingRequest> BookBorrowingRequestsAsApprover { get; set; } = new List<BookBorrowingRequest>();
    public IList<BookRating> BookRatings { get; set; }
}
