using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Messages;
public class BorrowingRequestMessage
{
    public static string ErrorInvalidDueDate(DateTime date) => $"The books can not be requested at date {date.ToString("MM/dd/yyyy")}";
    public static string ErrorOverLimitedBook(int limitedNumber) => $"The number of books can not be over {limitedNumber}";
    public static string ErrorOverLimitedRequest(int limitedNumber) => $"Your number of book requests for this month has reached its limit ({limitedNumber}).";

    public static string ErrorInvalidExpandedDueDate(DateTime date) => $"The extended date must be higher than ({date.ToString("MM/dd/yyyy")}).";

    public static string ErrorBookIsNotAvailable =  $"Some book is not availble or not existed!";
    public static string ErrorOverLimitedExpandedDate = $"Your number of book extended period requests has reached its limit !";

    public static string ErrorBookReturnCanNotUpdateToOtherStatus = $"The book you selected has been returned!";
}
