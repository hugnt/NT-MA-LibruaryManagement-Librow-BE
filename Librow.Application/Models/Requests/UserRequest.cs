using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Requests;
public class RegisterRequest
{
    public string Fullname { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}


public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class ExtendSessionRequest
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}




