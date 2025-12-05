using DrawingApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BoardShapeConfig : IEntityTypeConfiguration<BoardShape>
{
    public void Configure(EntityTypeBuilder<BoardShape> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.GeometryJson).IsRequired();
        b.Property(x => x.StrokeColor).IsRequired();
    }
}