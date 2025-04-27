using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Messages;
public class ErrorMessage
{
    public static string ConcurrencyConlict = "The data has been changed by someone else, please try again!";
    public static string ObjectNotFound(object value, string objName = "") => $"{objName} '{value}' is not existed!";
    public static string ObjectExisted(object value, string objName = "") => $"{objName} '{value}' is already existed!";

    public static string ObjectCanNotBeDeleted(object value, string objName = "") => $"{objName} '{value}' can not be deleted!";
    public static string ObjectCanNotBeUpdated(object value, string objName = "") => $"{objName} '{value}' can not be updated!";

    public static string ObjectCanNotBeNullOrEmpty(string objName = "") => $"{objName} can not be empty!";
    public static string ServerError() => $"Somethings was wrong, please try again!";
}
