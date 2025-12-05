using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp.Data.Repositories;


public class TemplateRepository : ITemplateRepository
{
    private readonly AppDbContext _db;
    public TemplateRepository(AppDbContext db) => _db = db;

    public Task<List<ShapeTemplate>> GetAllAsync()
        => _db.Templates.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<ShapeTemplate?> GetByIdAsync(Guid id)
        => _db.Templates.FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(ShapeTemplate template)
    {
        _db.Templates.Add(template);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var t = await _db.Templates.FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return;
        _db.Templates.Remove(t);
        await _db.SaveChangesAsync();
    }
}
