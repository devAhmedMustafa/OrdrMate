using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityConfig : IEntityTypeConfiguration<ItemAvailability>
{
    public void Configure(EntityTypeBuilder<ItemAvailability> builder)
    {
        builder.HasKey(x => new { x.ItemId, x.BranchId });
        builder.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
    }
}