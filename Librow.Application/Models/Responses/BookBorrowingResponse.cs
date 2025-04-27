using Librow.Core.Enums;

namespace Librow.Application.Models.Responses;
public class BorrowingRequestResponse
{
    public Guid Id { get; set; }
    public string RequestorName { get; set; }
    public string ApproverName { get; set; }
    public string StatusName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BorrowingRequestDetailsResponse : BorrowingRequestResponse
{
    public List<BorrowingDetailsResponse> Details { get; set; }
    
}

public class BorrowingDetailsResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookName { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime ExtendedDueDate { get; set; }
}