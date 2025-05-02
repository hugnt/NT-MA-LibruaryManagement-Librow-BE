using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Helpers;
public class StatusHelper
{
    public static string GetStatusName(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Waiting => "Waiting",
            RequestStatus.Approved => "Approved",
            RequestStatus.Rejected => "Rejected",
            _ => "Undefined"
        };
    }
}
