using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Helpers;
public class RoleHelper
{
    public static string GetRoleName(Role role)
    {
        return role switch
        {
            Role.Admin => "Admin",
            Role.Customer => "Customer",
            _ => "Undefined"
        };
    }
}
