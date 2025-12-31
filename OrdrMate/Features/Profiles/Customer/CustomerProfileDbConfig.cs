using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdrMate.Core;

namespace OrdrMate.Features.Profiles.Customer;

public class CustomerProfileDbConfig : IDbConfig<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.HasKey(cp => cp.CustomerId);
        builder.Property(cp => cp.FullName).HasMaxLength(100);
        builder.Property(cp => cp.PhoneNumber).HasMaxLength(15);
        builder.HasOne(cp => cp.User)
            .WithOne()
            .HasForeignKey<CustomerProfile>(cp => cp.CustomerId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
    }
}