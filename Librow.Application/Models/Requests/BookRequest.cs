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

public class BookFilterRequest: FilterRequest
{
    public int MinAvailable { get; set; } = 0;
    public int MaxAvailable { get; set; } = -1;
    public double MinRating { get; set; } = 0;
    public double MaxRating { get; set; } = 5;
    public Guid? CategoryId { get; set; }
}