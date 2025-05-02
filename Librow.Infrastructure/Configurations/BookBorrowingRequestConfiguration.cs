using Librow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Configurations;
public sealed class BookBorrowingRequestConfiguration : IEntityTypeConfiguration<BookBorrowingRequest>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequest> builder)
    {
        builder.ToTable(nameof(BookBorrowingRequest));
        builder.HasKey(x => x.Id);

        builder
            .HasOne<User>(bbr => bbr.Requestor).WithMany(u => u.BookBorrowingRequestsAsRequestor)
            .HasForeignKey(x => x.RequestorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(bbr => bbr.ApproverId).IsRequired(false);

        builder
           .HasOne<User>(bbr => bbr.Approver).WithMany(u => u.BookBorrowingRequestsAsApprover)
           .HasForeignKey(x => x.ApproverId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status).IsRequired(); 
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}

public sealed class BookBorrowingRequestDetailsConfiguration : IEntityTypeConfiguration<BookBorrowingRequestDetails>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequestDetails> builder)
    {
        builder.ToTable(nameof(BookBorrowingRequestDetails));
        builder.HasKey(x => x.Id);

        builder
            .HasOne<BookBorrowingRequest>(bbrd => bbrd.BookBorrowingRequest).WithMany(bbr => bbr.BookBorrowingRequestDetails)
            .HasForeignKey(x => x.RequestId);

        builder
           .HasOne<Book>(bbr => bbr.Book).WithMany(b => b.BookBorrowingRequestDetails)
           .HasForeignKey(x => x.BookId);
    }
}

