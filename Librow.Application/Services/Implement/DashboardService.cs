using Librow.Application.Models;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Services.Implement;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Book> _bookRepository;
    public DashboardService(IRepository<Book> bookRepository)
    {
        _bookRepository = bookRepository;
    }
    public async Task<Result> GetSummaryInfo()
    {
        const string sql = @"
            SELECT 
                (SELECT COUNT(*) FROM Book) AS BookCount,
                (SELECT COALESCE(SUM(Quantity), 0) FROM Book) AS BookTotalQuantity,
                (SELECT COALESCE(SUM(Available), 0) FROM Book) AS BookTotalAvailable,
                (SELECT COUNT(*) FROM BookCategory) AS BookCategoryCount,
                (SELECT COUNT(*) FROM [User] WHERE Role = @p0) AS UserCount,
                (SELECT COUNT(*) FROM BookBorrowingRequest) AS RequestCount";

        var queryResult = await _bookRepository.ExecuteRawSqlSingleAsync<SummaryQueryResult>(sql, (int)Core.Enums.Role.Customer);

        if (queryResult == null)
        {
            return Result<List<DashboardModel>>.SuccessWithBody([]);
        }

        var res = new List<DashboardModel>
        {
            new("Books", queryResult.BookCount, $"with {queryResult.BookTotalQuantity} in total and {queryResult.BookTotalAvailable} books available"),
            new("Categories", queryResult.BookCategoryCount - 1, $"Categories of books"),
            new("Customers", queryResult.UserCount, $"accounts"),
            new("Borrowing request", queryResult.RequestCount, $"include waiting, approved and rejected"),
        };

        return Result<List<DashboardModel>>.SuccessWithBody(res);

    }

    public async Task<Result> GetPopularBooks(int top = 5)
    {
        const string sql = @"WITH TopBooks AS (
                            SELECT 
                                b.Id,
                                b.Title,
                                COUNT(brd.Id) AS TotalRequest,
                                DENSE_RANK() OVER (ORDER BY COUNT(brd.Id) DESC) AS Rank
                            FROM Book b
                            LEFT JOIN BookBorrowingRequestDetails brd ON b.Id = brd.BookId
                            GROUP BY b.Id, b.Title
                        )
                        SELECT Id, Title, TotalRequest FROM TopBooks
                                        WHERE TotalRequest >= (
                                            SELECT MIN(TotalRequest)
                                            FROM TopBooks
                                            WHERE Rank <= @p0
                                        )
                        ORDER BY TotalRequest DESC ";

        var queryResult = await _bookRepository.ExecuteRawSqlAsync<BookTrendQueryResult>(sql, top);
        var res = new List<DashboardModel>();

        foreach (var item in queryResult)
        {
            res.Add(new(item.Title, item.TotalRequest));
        }

        return Result<List<DashboardModel>>.SuccessWithBody(res);
    }

    public async Task<Result> GetRequestAnalysis()
    {
        const string sql = @"
            SELECT 
                (SELECT COUNT(*) FROM BookBorrowingRequest WHERE status = 1) AS ApprovedNumber,
                (SELECT COUNT(*) FROM BookBorrowingRequest WHERE status = 2) AS RejectedNumber";

        var queryResult = await _bookRepository.ExecuteRawSqlSingleAsync<RequestStatusQueryResult>(sql, (int)Core.Enums.Role.Customer);
        if (queryResult == null)
        {
            return Result<List<DashboardModel>>.SuccessWithBody([]);
        }
        var res = new List<DashboardModel>()
        {
            new("Approved request", queryResult.ApprovedNumber),
            new("Rejected request", queryResult.RejectedNumber),
        };
        return Result<List<DashboardModel>>.SuccessWithBody(res);

    }

}
