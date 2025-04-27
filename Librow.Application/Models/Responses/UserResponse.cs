using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Responses;


public class LoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public UserResponse User { get; set; }

}
public class UserResponse
{
    public Guid Id { get; set; }
    public string Fullname { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }

}
