using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Messages;
public class BookMessage
{
    public static string QuantityCanNotBeLowerThanBorrowingNumber = "The quantity can not be lower than borrowing number!";

    public static string BookExistedInOtherProcess = "Action failure, This book in a request or borrowed by customer!";
}
