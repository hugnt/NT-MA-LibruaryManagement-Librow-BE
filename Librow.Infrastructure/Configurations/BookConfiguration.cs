using Librow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Configurations;
public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable(nameof(Book));
        builder.HasKey(x => x.Id);

        builder
            .HasOne<BookCategory>(b => b.BookCategory).WithMany(bc => bc.Books)
            .HasForeignKey(b => b.CategoryId);


        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Quantity).IsRequired();
    }
}
