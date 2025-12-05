using DrawingApp.Core.Entities;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DrawingApp.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Profiles.AnyAsync())
        {
            db.Profiles.Add(new Profile
            {
                Name = "Default Profile",
                Theme = AppTheme.System,
                DefaultBoardWidth = AppDefaults.DefaultBoardWidth,
                DefaultBoardHeight = AppDefaults.DefaultBoardHeight,
                DefaultStrokeColor = "#FF0F172A",
                DefaultStrokeThickness = 2,
                DefaultStrokeDash = StrokeDash.Solid
            });

            db.Profiles.Add(new Profile
            {
                Name = "Dark Artist",
                Theme = AppTheme.Dark,
                DefaultBoardWidth = 1000,
                DefaultBoardHeight = 700,
                DefaultStrokeColor = "#FFFFFFFF",
                DefaultStrokeThickness = 3,
                DefaultStrokeDash = StrokeDash.Dash
            });

            await db.SaveChangesAsync();
        }

        if (!await db.Templates.AnyAsync())
        {
            db.Templates.Add(new ShapeTemplate
            {
                Name = "Basic Rectangle",
                ShapeType = ShapeType.Rectangle,
                StrokeColor = "#FF2563EB",
                Thickness = 2,
                DashStyle = StrokeDash.Solid,
                GeometryJson = """{"x":50,"y":50,"w":180,"h":120}"""
            });

            await db.SaveChangesAsync();
        }
    }
}
