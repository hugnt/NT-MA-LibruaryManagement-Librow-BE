using Librow.Application.Helpers;
using Librow.Application.Models.Requests;
using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        Role = registerRequest.Role,
    };

    public static void MappingFieldFrom(this User trackingEntity, UserUpdateRequest updatedEntity)
    {
        trackingEntity.Fullname = updatedEntity.Fullname;
        trackingEntity.Email = updatedEntity.Email;
        trackingEntity.Username = updatedEntity.Username;
        trackingEntity.Role = updatedEntity.Role;
    }


    public static UserResponse ToResponse(this User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Fullname = user.Fullname,
        Role = user.Role,
    };

    public static Expression<Func<User, UserResponse>> SelectResponseExpression = x => new UserResponse
    {
        Id = x.Id,
        Username = x.Username,
        Email = x.Email,
        Fullname = x.Fullname,
        Role = x.Role,
    };

    public static Expression<Func<AuditLog, AuditLogResponse>> SelectAuditLogResponseExpression = x => new AuditLogResponse
    {
        UserId = x.UserId,
        Fullname = x.User.Fullname,
        Role = x.User.Role,
        Action = x.Action,
        EntityName = x.EntityName,
        EntityId = x.EntityId,
        OldValues = x.OldValues,
        NewValues = x.NewValues,
        CreatedAt = x.CreatedAt
    };
}
