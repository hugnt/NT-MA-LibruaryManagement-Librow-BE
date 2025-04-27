using Librow.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Configurations;
public sealed class BookRatingConfiguration : IEntityTypeConfiguration<BookRating>
{
    public void Configure(EntityTypeBuilder<BookRating> builder)
    {
        builder.ToTable(nameof(BookRating));
        builder.HasKey(x => x.Id);

        builder
            .HasOne<User>(u => u.Reviewer).WithMany(br => br.BookRatings)
            .HasForeignKey(x => x.ReviewerId);

        builder
            .HasOne<Book>(u => u.Book).WithMany(br => br.BookRatings)
            .HasForeignKey(x => x.BookId);
    }
}
