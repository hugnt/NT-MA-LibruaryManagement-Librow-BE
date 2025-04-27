using Librow.Core.Entities;
using Librow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Common;
public static class SeedData
{
    private static readonly DateTime _defaultTime = new(2025, 01, 01);

    public static BookCategory DefaultBookCategory => new BookCategory
    {
        Id = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
        Name = "No Category",
        CreatedAt = _defaultTime,
        UpdatedAt = _defaultTime
    };

    public static IEnumerable<User> Users => new List<User>
    {
        new User
        {
            Id = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),
            Fullname = "Admin",
            Username = "admin",
            Role = Role.Admin,
            PasswordHash = "62D97E720D5574BBEB80B41144D1BC86648C78D747DDD4078C62E1E279B4D94D-F75BF08670B03F19CABE8AAD26B5763F",
            Email = "thanh.hung.st302@gmail.com",
            CreatedAt = _defaultTime,
            UpdatedAt = _defaultTime
        },
        new User
        {
            Id = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),
            Fullname = "Nguyen Thanh User",
            Username = "user001",
            Role = Role.Customer,
            PasswordHash = "C40D0CF1F0815D27829F76BA3F7B0399A9FF5BD6C05252B7F500B6826419EE25-E41A6B82F54C202A240A483B224F15C3",
            Email = "thanhhungst314@gmail.com",
            CreatedAt = _defaultTime,
            UpdatedAt = _defaultTime
        }
    };
}
