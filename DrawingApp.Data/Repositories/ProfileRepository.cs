using DrawingApp.Core.Entities;
using DrawingApp.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.Data.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProfileRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Profile>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Profiles.AsNoTracking().ToListAsync();
    }

    public async Task<Profile?> GetByIdAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Profiles.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Profile profile)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Profiles.Add(profile);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Profile profile)
    {
        await using var db = await _factory.CreateDbContextAsync();

        // attach & mark modified
        db.Profiles.Update(profile);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var entity = await db.Profiles.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return;

        db.Profiles.Remove(entity);
        await db.SaveChangesAsync();
    }
}
