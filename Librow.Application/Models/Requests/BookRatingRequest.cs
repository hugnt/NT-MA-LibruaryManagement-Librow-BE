using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Requests;
public class BookRatingRequest
{
    public Guid BookId { get; set; }
    public string Comment { get; set; }
    public double Rate { get; set; }
}
