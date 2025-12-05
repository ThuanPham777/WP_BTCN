using DrawingApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ShapeTemplateConfig : IEntityTypeConfiguration<ShapeTemplate>
{
    public void Configure(EntityTypeBuilder<ShapeTemplate> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(120);
        b.Property(x => x.GeometryJson).IsRequired();
    }
}