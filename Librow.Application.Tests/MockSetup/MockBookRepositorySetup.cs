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
public class MockBookRepositorySetup
{

    public static List<Book> ListBooks(){
        var res = new List<Book>()
        {
            new() { Id = Guid.Parse("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1"), Title = "Physics 101", Author = "Isaac Newton", CategoryId = MockBookCategoryRepositorySetup.PhysicsId, Quantity = 10, Available = 7, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-12), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("e2e2e2e2-e2e2-e2e2-e2e2-e2e2e2e2e2e2"), Title = "Basic Chemistry", Author = "Marie Curie", CategoryId = MockBookCategoryRepositorySetup.ChemistryId, Quantity = 12, Available = 12, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-11), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("f3f3f3f3-f3f3-f3f3-f3f3-f3f3f3f3f3f3"), Title = "Biology for Beginners", Author = "Charles Darwin", CategoryId = MockBookCategoryRepositorySetup.BiologyId, Quantity = 8, Available = 5, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("a4a4a4a4-a4a4-a4a4-a4a4-a4a4a4a4a4a4"), Title = "Economic Theories", Author = "Adam Smith", CategoryId = MockBookCategoryRepositorySetup.EconomicsId, Quantity = 15, Available = 12, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-9), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("b5b5b5b5-b5b5-b5b5-b5b5-b5b5b5b5b5b5"), Title = "Understanding Psychology", Author = "Sigmund Freud", CategoryId = MockBookCategoryRepositorySetup.PsychologyId, Quantity = 9, Available = 6, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-8), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6"), Title = "World Literature", Author = "William Shakespeare", CategoryId = MockBookCategoryRepositorySetup.LiteratureId, Quantity = 6, Available = 4, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-7), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("d7d7d7d7-d7d7-d7d7-d7d7-d7d7d7d7d7d7"), Title = "Philosophy Basics", Author = "Plato", CategoryId = MockBookCategoryRepositorySetup.PhilosophyId, Quantity = 5, Available = 3, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-6), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("e8e8e8e8-e8e8-e8e8-e8e8-e8e8e8e8e8e8"), Title = "Mathematics Explained", Author = "Euclid", CategoryId = MockBookCategoryRepositorySetup.MathId, Quantity = 11, Available = 10, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-5), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("f9f9f9f9-f9f9-f9f9-f9f9-f9f9f9f9f9f9"), Title = "Modern Art", Author = "Leonardo da Vinci", CategoryId = MockBookCategoryRepositorySetup.ArtId, Quantity = 7, Available = 3, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-4), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0"), Title = "History of the World", Author = "Herodotus", CategoryId = MockBookCategoryRepositorySetup.HistoryId, Quantity = 13, Available = 11, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1"), Title = "Tech Innovations", Author = "Alan Turing", CategoryId = MockBookCategoryRepositorySetup.TechnologyId, Quantity = 14, Available = 8, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] },
            new() { Id = Guid.Parse("c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2"), Title = "The Science World", Author = "Stephen Hawking", CategoryId = MockBookCategoryRepositorySetup.ScienceId, Quantity = 10, Available = 10, CreatedBy = Guid.Empty, UpdatedBy = Guid.Empty, CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow, IsDeleted = false, BookRatings = new List<BookRating>(), BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>(), BookCategory = null!, RowVersion = [] }
        };

        foreach (var book in res)
        {
            book.BookCategory = MockBookCategoryRepositorySetup.ListCategories().First(x => x.Id == book.CategoryId)!;
            book.BookRatings = MockBookRatingRepositorySetup.ListRatings().Where(x => x.BookId == book.Id).ToList();
        }
        return res;

     }

}
