using Librow.Application.Models.Responses;
using Librow.Core.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Tests.MockSetup;
public class MockBookRatingRepositorySetup
{
    public static readonly Guid Rating1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    public static readonly Guid Rating2Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
    public static readonly Guid Rating3Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
    public static readonly Guid Rating4Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4");
    public static readonly Guid Rating5Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5");
    public static readonly Guid Rating6Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6");
    public static readonly Guid Rating7Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7");
    public static readonly Guid Rating8Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa8");
    public static readonly Guid Rating9Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa9");
    public static readonly Guid Rating10Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10");

    public static List<BookRating> ListRatings() => new()
    {
        new() { Id = Rating1Id, BookId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ReviewerId = Guid.NewGuid(), Comment = "Great!", Rate = 4.5, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating2Id, BookId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ReviewerId = Guid.NewGuid(), Comment = "Excellent", Rate = 5.0, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-9), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating3Id, BookId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ReviewerId = Guid.NewGuid(), Comment = "Not bad", Rate = 3.5, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-8), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating4Id, BookId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ReviewerId = Guid.NewGuid(), Comment = "Could be better", Rate = 2.5, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-7), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating5Id, BookId = Guid.Parse("33333333-3333-3333-3333-333333333333"), ReviewerId = Guid.NewGuid(), Comment = "Average", Rate = 3.0, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-6), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating6Id, BookId = Guid.Parse("33333333-3333-3333-3333-333333333333"), ReviewerId = Guid.NewGuid(), Comment = "Loved it", Rate = 4.8, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-5), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating7Id, BookId = Guid.Parse("44444444-4444-4444-4444-444444444444"), ReviewerId = Guid.NewGuid(), Comment = "Too long", Rate = 2.0, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-4), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating8Id, BookId = Guid.Parse("44444444-4444-4444-4444-444444444444"), ReviewerId = Guid.NewGuid(), Comment = "Nice read", Rate = 4.0, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating9Id, BookId = Guid.Parse("55555555-5555-5555-5555-555555555555"), ReviewerId = Guid.NewGuid(), Comment = "Poor writing", Rate = 1.5, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null },
        new() { Id = Rating10Id, BookId = Guid.Parse("55555555-5555-5555-5555-555555555555"), ReviewerId = Guid.NewGuid(), Comment = "Very informative", Rate = 4.2, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Book = null, Reviewer = null }
    };

}
