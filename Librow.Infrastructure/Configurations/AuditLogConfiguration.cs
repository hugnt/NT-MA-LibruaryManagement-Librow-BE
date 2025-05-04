using Librow.Core.Entities;
using Librow.Core.Enums;
using Librow.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Configurations;
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(nameof(AuditLog));
        builder.HasKey(x => x.Id);

        builder
           .HasOne(a => a.User)
           .WithMany()
           .HasForeignKey(a => a.UserId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
