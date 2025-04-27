using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Mappings;
public static class BookCategoryMapping
{
    public static BookCategory ToEntity(this BookCategoryRequest bookCategoryRequest) => new()
    {
        Name = bookCategoryRequest.Name,
    };

    public static void MappingFieldFrom(this BookCategory trackingEntity, BookCategoryRequest updatedEntity)
    {
        trackingEntity.Name = updatedEntity.Name;
    }

    public static BookCategoryResponse ToResponse(this BookCategory bookCategory) => new()
    {
        Id = bookCategory.Id,
        Name = bookCategory.Name,
    };
}
