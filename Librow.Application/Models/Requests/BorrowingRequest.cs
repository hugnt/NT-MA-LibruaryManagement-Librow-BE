using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Requests;


public class BorrowingRequestFilter : FilterRequest
{
    public RequestStatus Status { get; set; }
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