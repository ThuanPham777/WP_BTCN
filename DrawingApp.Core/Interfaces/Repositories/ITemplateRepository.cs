using DrawingApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.Core.Interfaces.Repositories;

public interface ITemplateRepository
{
    Task<List<ShapeTemplate>> GetAllAsync();
    Task<ShapeTemplate?> GetByIdAsync(Guid id);
    Task AddAsync(ShapeTemplate template);
    Task DeleteAsync(Guid id);
}
