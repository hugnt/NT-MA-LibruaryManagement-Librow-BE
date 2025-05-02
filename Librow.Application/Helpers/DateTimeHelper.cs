using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Helpers;
public static class DateTimeHelper
{
    public static DateTime FromUnixTimeToDateTime(this long unixTime)
    {
        var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTimeInterval.AddSeconds(unixTime).ToUniversalTime();
        return dateTimeInterval;

    }

    public static bool IsEqualDateExact(this DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date && date1.Month == date2.Month && date1.Year == date2.Year;
    }

    public static bool IsDateBetween(this DateTime thisDate, DateTime startDate, DateTime endDate)
    {
        return thisDate >= startDate && thisDate <= endDate;
    }
}
