using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Responses;

public class ReviewModel
{
    public Guid Id { get; set; }
    public string ReviewerName { get; set; }
    public string Comment { get; set; }
    public double Rate { get; set; }
    public string CommentTime { get; set; }
}
public class BookRatingResponse
{
    public List<ReviewModel> Reviews { get; set; }
    public double AverageRating { get; set; }
}
