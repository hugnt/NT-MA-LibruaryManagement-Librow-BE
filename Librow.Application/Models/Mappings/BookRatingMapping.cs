using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Mappings;
public static class BookRatingMapping
{
    public static BookRating ToEntity(this BookRatingRequest bookRatingRequest) => new()
    {
        BookId = bookRatingRequest.BookId,
        Comment = bookRatingRequest.Comment,
        Rate = bookRatingRequest.Rate
    };

    public static Expression<Func<BookRating, ReviewModel>> SelectModelExpression = x => new ReviewModel
    {
        Id = x.Id,
        ReviewerName = x.Reviewer.Fullname,
        Comment = x.Comment,
        Rate = x.Rate,
    };
}
