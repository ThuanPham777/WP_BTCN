using DrawingApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrawingApp.Data.Configurations;
public class DrawingBoardConfig : IEntityTypeConfiguration<DrawingBoard>
{
    public void Configure(EntityTypeBuilder<DrawingBoard> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(120);
        b.Property(x => x.BackgroundColor).IsRequired();
        b.HasMany(x => x.Shapes)
         .WithOne(x => x.Board)
         .HasForeignKey(x => x.BoardId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}