using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models;
public class FilterRequest
{
    public int? PageSize { get; set; }
    public int? PageNumber { get; set; } 
    public string? OrderBy { get; set; }
    public string? SearchValue { get; set; }
}

public class FilterResult<T> : Result<T>
{
    public int TotalRecords { get; set; }

    public static FilterResult<T> Success(T body, int totalRecords) => new() { IsSuccess = true, StatusCode = HttpStatusCode.OK, Message = "Ok", Data = body, TotalRecords = totalRecords };
}
