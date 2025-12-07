using DrawingApp.Core.Entities;
using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        // -------------------------
        // 1) Profiles
        // -------------------------
        if (!await db.Profiles.AnyAsync())
        {
            var defaultProfile = new Profile
            {
                Name = "Default Profile",
                Theme = AppTheme.System,
                DefaultBoardWidth = AppDefaults.DefaultBoardWidth,
                DefaultBoardHeight = AppDefaults.DefaultBoardHeight,
                DefaultStrokeColor = "#FF0F172A",
                DefaultStrokeThickness = 2,
                DefaultStrokeDash = StrokeDash.Solid
            };

            var darkArtist = new Profile
            {
                Name = "Dark Artist",
                Theme = AppTheme.Dark,
                DefaultBoardWidth = 1000,
                DefaultBoardHeight = 700,
                DefaultStrokeColor = "#FFFFFFFF",
                DefaultStrokeThickness = 3,
                DefaultStrokeDash = StrokeDash.Dash
            };

            db.Profiles.AddRange(defaultProfile, darkArtist);
            await db.SaveChangesAsync();
        }

        var defaultP = await db.Profiles
            .OrderBy(p => p.CreatedAt)
            .FirstAsync();

        // -------------------------
        // 2) Templates
        // -------------------------
        if (!await db.Templates.AnyAsync())
        {
            db.Templates.AddRange(
                new ShapeTemplate
                {
                    Name = "Basic Rectangle",
                    ShapeType = ShapeType.Rectangle,
                    StrokeColor = "#FF2563EB",
                    FillColor = null,
                    Thickness = 2,
                    DashStyle = StrokeDash.Solid,
                    GeometryJson = """{"x":50,"y":50,"w":180,"h":120}"""
                },
                new ShapeTemplate
                {
                    Name = "Basic Line",
                    ShapeType = ShapeType.Line,
                    StrokeColor = "#FF0F172A",
                    FillColor = null,
                    Thickness = 2,
                    DashStyle = StrokeDash.Solid,
                    GeometryJson = """{"x1":40,"y1":40,"x2":240,"y2":120}"""
                },
                new ShapeTemplate
                {
                    Name = "Oval Outline",
                    ShapeType = ShapeType.Oval,
                    StrokeColor = "#FF7C3AED",
                    FillColor = null,
                    Thickness = 2,
                    DashStyle = StrokeDash.Solid,
                    GeometryJson = """{"x":80,"y":80,"w":160,"h":100}"""
                },
                new ShapeTemplate
                {
                    Name = "Triangle",
                    ShapeType = ShapeType.Triangle,
                    StrokeColor = "#FF16A34A",
                    FillColor = null,
                    Thickness = 2,
                    DashStyle = StrokeDash.Dash,
                    GeometryJson = """{"points":[{"x":120,"y":40},{"x":40,"y":180},{"x":200,"y":180}]}"""
                }
            );

            await db.SaveChangesAsync();
        }

        // -------------------------
        // 3) Demo Board + Shapes
        // -------------------------
        if (!await db.Boards.AnyAsync())
        {
            var demoBoard = new DrawingBoard
            {
                ProfileId = defaultP.Id,
                Name = "Demo Board",
                Width = defaultP.DefaultBoardWidth,
                Height = defaultP.DefaultBoardHeight,
                BackgroundColor = "#FFFFFFFF",
                Shapes = new List<BoardShape>
                {
                    new BoardShape
                    {
                        ShapeType = ShapeType.Line,
                        StrokeColor = "#FF0F172A",
                        FillColor = null,
                        Thickness = 2,
                        DashStyle = StrokeDash.Solid,
                        GeometryJson = """{"x1":30,"y1":30,"x2":280,"y2":90}"""
                    },
                    new BoardShape
                    {
                        ShapeType = ShapeType.Rectangle,
                        StrokeColor = "#FF2563EB",
                        FillColor = null,
                        Thickness = 2,
                        DashStyle = StrokeDash.Solid,
                        GeometryJson = """{"x":80,"y":140,"w":200,"h":120}"""
                    },
                    new BoardShape
                    {
                        ShapeType = ShapeType.Oval,
                        StrokeColor = "#FF7C3AED",
                        FillColor = "#227C3AED",
                        Thickness = 3,
                        DashStyle = StrokeDash.Solid,
                        GeometryJson = """{"x":330,"y":120,"w":160,"h":110}"""
                    }
                }
            };

            db.Boards.Add(demoBoard);
            await db.SaveChangesAsync();
        }
    }
}
