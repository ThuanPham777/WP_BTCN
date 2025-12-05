using DrawingApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrawingApp.Data.Configurations;

public class ProfileConfig : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.DefaultStrokeColor).IsRequired();
        b.HasMany(x => x.Boards)
         .WithOne(x => x.Profile)
         .HasForeignKey(x => x.ProfileId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}