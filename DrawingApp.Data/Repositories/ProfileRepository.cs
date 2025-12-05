using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp.Data.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly AppDbContext _db;
    public ProfileRepository(AppDbContext db) => _db = db;

    public Task<List<Profile>> GetAllAsync()
        => _db.Profiles.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync();

    public Task<Profile?> GetByIdAsync(Guid id)
        => _db.Profiles.Include(x => x.Boards).FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(Profile profile)
    {
        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Profile profile)
    {
        _db.Profiles.Update(profile);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var p = await _db.Profiles.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return;
        _db.Profiles.Remove(p);
        await _db.SaveChangesAsync();
    }
}