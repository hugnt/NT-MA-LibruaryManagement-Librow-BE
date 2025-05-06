using Librow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Tests.MockSetup;
public static class MockAuditLogRepositorySetup
{
    public static List<AuditLog> ListAuditLogs()
    {
        var res = new List<AuditLog>()
        {
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),  // Admin
                Action = "Create",
                EntityName = "Book",
                EntityId = "1",
                OldValues = null,
                NewValues = "{ \"Title\": \"New Book\", \"Author\": \"John Doe\" }",
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),  // User001
                Action = "Update",
                EntityName = "BookCategory",
                EntityId = "2",
                OldValues = "{ \"Name\": \"Fiction\" }",
                NewValues = "{ \"Name\": \"Science Fiction\" }",
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),  // User001
                Action = "Delete",
                EntityName = "Book",
                EntityId = "3",
                OldValues = "{ \"Title\": \"Old Book\", \"Author\": \"Jane Smith\" }",
                NewValues = null,
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c48"),  // User002
                Action = "Create",
                EntityName = "User",
                EntityId = "4",
                OldValues = null,
                NewValues = "{ \"Fullname\": \"Nguyen Thanh User 2\", \"Username\": \"user00 2\" }",
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c48"),  // User002
                Action = "Update",
                EntityName = "User",
                EntityId = "5",
                OldValues = "{ \"Role\": \"Customer\" }",
                NewValues = "{ \"Role\": \"Admin\" }",
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c45"),  // User003
                Action = "Delete",
                EntityName = "Book",
                EntityId = "6",
                OldValues = "{ \"Title\": \"Old Book 2\", \"Author\": \"Mike Lee\" }",
                NewValues = null,
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),  // Admin
                Action = "Update",
                EntityName = "BookCategory",
                EntityId = "7",
                OldValues = "{ \"Name\": \"Non-fiction\" }",
                NewValues = "{ \"Name\": \"Biography\" }",
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c4d"),  // User001
                Action = "Create",
                EntityName = "Book",
                EntityId = "8",
                OldValues = null,
                NewValues = "{ \"Title\": \"New Book Title\", \"Author\": \"Johnathan Lee\" }",
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("9a4c6e2d-8f3b-4d1a-b5c7-2e9f0a1b3c45"),  // User003
                Action = "Delete",
                EntityName = "User",
                EntityId = "9",
                OldValues = "{ \"Fullname\": \"Nguyen Thanh User 3\" }",
                NewValues = null,
                CreatedAt = DateTime.Now
            },
            new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("3f8b2a1e-5c4d-4e9f-a2b3-7c8d9e0f1a2b"),  // Admin
                Action = "Update",
                EntityName = "AuditLog",
                EntityId = "10",
                OldValues = "{ \"Action\": \"Create\" }",
                NewValues = "{ \"Action\": \"Updated\" }",
                CreatedAt = DateTime.Now
            }
        };

        foreach (var detail in res)
        {
            detail.User = MockUserRepositorySetup.ListUsers().First(x => x.Id == detail.UserId);
            
        }
        return res;
    }
}
