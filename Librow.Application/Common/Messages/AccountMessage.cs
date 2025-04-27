using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Messages;
public class AccountMessage
{
    public const string LoginSuccessfully = $"Login Successfully!";
    public const string RegisterSuccessfully = $"Register Successfully!";

    public const string LoginFailure = $"Login Failure!";
    public const string RegisterFailure = $"Register Failure!";
    public const string PasswordNotCorrect = $"Password is not correct!";
}
