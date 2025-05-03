using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Mappings;
public static class BookMapping
{
    public static Book ToEntity(this BookRequest bookRequest) => new()
    {
        Title = bookRequest.Title,
        CategoryId = bookRequest.CategoryId,
        Author = bookRequest.Author,
        Quantity = bookRequest.Quantity
    };

    public static void MappingFieldFrom(this Book trackingEntity, BookRequest updatedEntity)
    {
        trackingEntity.Title = updatedEntity.Title;
        trackingEntity.CategoryId = updatedEntity.CategoryId;
        trackingEntity.Author = updatedEntity.Author;
        trackingEntity.Quantity = updatedEntity.Quantity;
    }

    public static BookResponse ToResponse(this Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        CategoryId = book.CategoryId,
        CategoryName = book.BookCategory.Name,
        Author = book.Author,
        Quantity = book.Quantity,
        Available = book.Available
    };

    public static Expression<Func<Book, BookResponse>> SelectResponseExpression = x => new BookResponse
    {
        Id = x.Id,
        Title = x.Title,
        CategoryId = x.CategoryId,
        CategoryName = x.BookCategory.Name,
        Author = x.Author,
        Quantity = x.Quantity,
        Available = x.Available
    };
}
