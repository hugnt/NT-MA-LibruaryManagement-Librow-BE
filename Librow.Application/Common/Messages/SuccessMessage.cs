using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Messages;
public class SuccessMessage
{


    public static string CreatedSuccessfully(string objName = "") => $"Added {objName} Successfully!";
    public static string UpdatedSuccessfully(string objName = "") => $"Updated {objName} Successfully!";
    public static string UpdatedSuccessfully(Guid id, string objName = "") => $"Updated {objName} with id = {id} Successfully!";
    public static string DeletedSuccessfully(Guid id, string objName = "") => $"Deleted {objName} with id = {id} Successfully!";
    public static string DeletedSuccessfully(string objName = "") => $"Deleted {objName} with Successfully!";

}
