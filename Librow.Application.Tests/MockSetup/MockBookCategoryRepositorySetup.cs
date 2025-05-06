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
public class MockBookCategoryRepositorySetup
{
    public static readonly Guid ScienceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TechnologyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid HistoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ArtId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid MathId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid PhilosophyId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid LiteratureId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid PsychologyId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid EconomicsId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static readonly Guid BiologyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid ChemistryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid PhysicsId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static List<BookCategory> ListCategories() => new()
    {
        new() { Id = ScienceId, Name = "Science", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-12), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = TechnologyId, Name = "Technology", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-11), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = HistoryId, Name = "History", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = ArtId, Name = "Art", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-9), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = MathId, Name = "Mathematics", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-8), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = PhilosophyId, Name = "Philosophy", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-7), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = LiteratureId, Name = "Literature", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-6), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = PsychologyId, Name = "Psychology", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-5), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = EconomicsId, Name = "Economics", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-4), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = BiologyId, Name = "Biology", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = ChemistryId, Name = "Chemistry", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() },
        new() { Id = PhysicsId, Name = "Physics", CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow, IsDeleted = false, Books = new List<Book>() }
    };

    public static BookCategory DefaultCategory => ListCategories().FirstOrDefault()!;

}
