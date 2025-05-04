using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Requests;

public class RequestFilter
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class BorrowingRequestFilter : FilterRequest
{
    public RequestStatus? Status { get; set; }
}
public class BorrowingDetailsRequest
{
    public Guid BookId { get; set; }
    public DateTime DueDate { get; set; }
}
public class BorrowingRequest
{
    public List<BorrowingDetailsRequest> Details { get; set; }

}
public class ExtendBorrowingRequest
{
    public DateTime ExtendedDueDate { get; set; }

}
public class UpdateStatusRequest
{
    public RequestStatus Status { get; set; }

}
public class UpdateBorrowingStatusRequest
{
    public BorrowingStatus Status { get; set; }

}

public class OverdueRequest
{
    public Guid RequestId { get; set; }
    public Guid RequestorId { get; set; }
    public string RequestorName { get; set; }
    public string RequestorEmail { get; set; }
    public DateTime ExtendedDueDate { get; set; }
    public string BookName { get; set; }
}


public class OverdueRequestByUser
{
    public string RequestorName { get; set; }
    public string RequestorEmail { get; set; }
    public List<OverdueBook> OverdueBooks { get; set; }

}

public class OverdueBook
{
    public string BookName { get; set; }
    public DateTime ExtendedDueDate { get; set; }
    public int OverdueDays { get; set; }
}