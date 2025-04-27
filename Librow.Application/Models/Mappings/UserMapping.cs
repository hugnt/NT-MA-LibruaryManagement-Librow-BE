using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models.Mappings;
public static class UserMapping
{
    public static User ToEntity(this RegisterRequest registerRequest) => new()
    {
        Fullname = registerRequest.Fullname,
        Email = registerRequest.Email,
        Username = registerRequest.Username.ToLower().Trim(),
    };

    public static UserResponse ToResponse(this User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Fullname = user.Fullname,
        Role = user.Role
    };
}
