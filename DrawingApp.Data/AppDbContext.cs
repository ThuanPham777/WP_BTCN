using DrawingApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DrawingApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<DrawingBoard> Boards => Set<DrawingBoard>();
    public DbSet<BoardShape> BoardShapes => Set<BoardShape>();
    public DbSet<ShapeTemplate> Templates => Set<ShapeTemplate>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
