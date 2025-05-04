using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Responses;

public class DashboardModel
{
    public DashboardModel(string title, int total, string subContent)
    {
        Title = title;
        Total = total;
        SubContent = subContent;
    }

    public DashboardModel(string title, int total)
    {
        Title = title;
        Total = total;
    }

    public string Title { get; set; }
    public int Total { get; set; }
    public string? SubContent { get; set; }
}

public class SummaryQueryResult
{
    public int BookCount { get; set; }
    public int BookTotalQuantity { get; set; }
    public int BookTotalAvailable { get; set; }
    public int BookCategoryCount { get; set; }
    public int UserCount { get; set; }
    public int RequestCount { get; set; }
}

public class RequestStatusQueryResult
{
    public int ApprovedNumber { get; set; }
    public int RejectedNumber { get; set; }
}

public class BookTrendQueryResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalRequest { get; set; }
}
