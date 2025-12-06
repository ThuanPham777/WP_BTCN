using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp.Data.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public BoardRepository(IDbContextFactory<AppDbContext> factory)
        => _factory = factory;

    public async Task<List<DrawingBoard>> GetAllAsync()
    {
        using var db = _factory.CreateDbContext();
        return await db.Boards
            .AsNoTracking()
            .Include(x => x.Profile)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<DrawingBoard?> GetByIdAsync(Guid? id)
    {
        using var db = _factory.CreateDbContext();
        return await db.Boards
            .Include(x => x.Shapes)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(DrawingBoard board)
    {
        using var db = _factory.CreateDbContext();
        db.Boards.Add(board);
        await db.SaveChangesAsync();
    }

    public async Task UpdateContentAsync(
    Guid boardId,
    string? name,
    double width,
    double height,
    string? background,
    List<BoardShape> newShapes)
    {
        using var db = _factory.CreateDbContext();
        using var tx = await db.Database.BeginTransactionAsync();

        var board = await db.Boards
            .Include(b => b.Shapes)
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board == null)
            throw new InvalidOperationException("Board not found");

        // Update board fields
        board.Width = width;
        board.Height = height;
        board.BackgroundColor = background;
        board.Name = string.IsNullOrWhiteSpace(name) ? board.Name : name!.Trim();

        // Snapshot old shapes to remove safely
        var oldShapes = board.Shapes.ToList();
        if (oldShapes.Count > 0)
            db.BoardShapes.RemoveRange(oldShapes);

        // Build fresh shapes with explicit FK
        var fresh = newShapes.Select(s => new BoardShape
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,                
            ShapeType = s.ShapeType,
            StrokeColor = s.StrokeColor,
            FillColor = s.FillColor,
            Thickness = s.Thickness,
            DashStyle = s.DashStyle,
            GeometryJson = s.GeometryJson
        }).ToList();

        await db.BoardShapes.AddRangeAsync(fresh);

        try
        {
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            var exists = await db.Boards.AsNoTracking().AnyAsync(b => b.Id == boardId);
            if (!exists)
                throw new InvalidOperationException("Board no longer exists. Please reload list.");

            throw new InvalidOperationException(
                "Save conflict detected. Please reopen the board and try again.");
        }
    }
    public async Task UpdateAsync(DrawingBoard board)
    {
        using var db = _factory.CreateDbContext();

        var existing = await db.Boards
            .Include(x => x.Shapes)
            .FirstOrDefaultAsync(x => x.Id == board.Id);

        if (existing == null)
            throw new InvalidOperationException("Board not found");

        existing.Name = board.Name;
        existing.Width = board.Width;
        existing.Height = board.Height;
        existing.BackgroundColor = board.BackgroundColor;
        existing.ProfileId = board.ProfileId;

        db.BoardShapes.RemoveRange(existing.Shapes);
        existing.Shapes = board.Shapes.Select(s => new BoardShape
        {
            Id = Guid.NewGuid(),
            ShapeType = s.ShapeType,
            StrokeColor = s.StrokeColor,
            FillColor = s.FillColor,
            Thickness = s.Thickness,
            DashStyle = s.DashStyle,
            GeometryJson = s.GeometryJson
        }).ToList();

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        using var db = _factory.CreateDbContext();

        var board = await db.Boards
            .Include(x => x.Shapes)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (board == null) return;

        db.BoardShapes.RemoveRange(board.Shapes);
        db.Boards.Remove(board);

        await db.SaveChangesAsync();
    }
}
