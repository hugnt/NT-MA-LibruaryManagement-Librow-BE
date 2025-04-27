using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Extensions;
public static class QueryableExtensions_Tolearn
{
    // SO SÁNH VỚI TRUY VẤN:  .Where(x => x.Title == "Harry Potter");
    public static IQueryable<T> FilterDynamic_Tolearn<T>(this IQueryable<T> query, string propertyName, object value)
    {
        if (string.IsNullOrWhiteSpace(propertyName) || value == null) return query;

        var parameter = Expression.Parameter(typeof(T), "x"); // parameter x kiểu T: (x => ...)
        var property = Expression.Property(parameter, propertyName); // Lấy property propertyName từ x: x.Title
        var constant = Expression.Constant(value); //Tạo ra giá trị cần so sánh (hằng số) : "Harry Potter"

        var equal = Expression.Equal(property, Expression.Convert(constant, property.Type));// So sánh x.Title == "Harry Potter", Expression.Convert: đảm bảo constant cùng kiểu dữu liệu
        var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);// Tạo thành một lambda expression.: (x => x.Title == "Harry Potter")

        return query.Where(lambda);
    }

    public static IQueryable<T> OrderByDynamic_Tolearn<T>(this IQueryable<T> query, string propertyName, bool ascending)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        string methodName = ascending ? "OrderBy" : "OrderByDescending";

        var result = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                query.Expression,
                Expression.Quote(lambda)
        );

        return query.Provider.CreateQuery<T>(result);
    }

    // SO SÁNH VỚI TRUY VẤN:  .Where(x => x.Title.Contains("Harry");
    public static IQueryable<T> SearchDynamic_Tolearn<T>(this IQueryable<T> query, string propertyName, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(searchTerm)) return query;

        var parameter = Expression.Parameter(typeof(T), "x"); // parameter x kiểu T: (x => ...)
        var property = Expression.Property(parameter, propertyName); // Lấy property propertyName từ x: x.Title

        if (property.Type != typeof(string)) return query; // vì là so sánh LIKE nên chỉ cần value là string
        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) }); 
        var searchExpression = Expression.Call(property, method!, Expression.Constant(searchTerm));
        var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);

        return query.Where(lambda);
    }


}
