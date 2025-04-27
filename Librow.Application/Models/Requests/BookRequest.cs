using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Requests;
public class BookRequest
{
    public string Title { get; set; }
    public Guid CategoryId { get; set; }
    public string Author { get; set; }
    public int Quantity { get; set; }

}
