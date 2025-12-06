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
    private readonly AppDbContext _db;
    public BoardRepository(AppDbContext db) => _db = db;

    public Task<List<DrawingBoard>> GetAllAsync()
        => _db.Boards.AsNoTracking().Include(x => x.Profile).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<DrawingBoard?> GetByIdAsync(Guid? id)
        => _db.Boards.Include(x => x.Shapes).FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(DrawingBoard board)
    {
        _db.Boards.Add(board);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(DrawingBoard board)
    {
        _db.Boards.Update(board);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var b = await _db.Boards.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return;
        _db.Boards.Remove(b);
        await _db.SaveChangesAsync();
    }
}