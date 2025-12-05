using DrawingApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.Core.Interfaces.Repositories;

public interface IProfileRepository
{
    Task<List<Profile>> GetAllAsync();
    Task<Profile?> GetByIdAsync(Guid id);
    Task AddAsync(Profile profile);
    Task UpdateAsync(Profile profile);
    Task DeleteAsync(Guid id);
}