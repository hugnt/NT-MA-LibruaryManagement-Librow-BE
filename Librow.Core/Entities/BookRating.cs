using Librow.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Core.Entities;
public class BookRating : Entity, IAuditableEntity
{
    public Guid BookId { get; set; }
    public Guid ReviewerId { get; set; }
    public string Comment { get; set; }
    public double Rate { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Book Book { get; set; }
    public User Reviewer { get; set; }
}
