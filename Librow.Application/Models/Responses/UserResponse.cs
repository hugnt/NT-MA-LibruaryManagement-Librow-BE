﻿using Librow.Core.Enums;
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
    public string RoleName { get; set; }
}

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }

}


public class AuditLogResponse
{
    public Guid? UserId { get; set; }
    public string? Fullname { get; set; }
    public Role? Role { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime? CreatedAt { get; set; }

}