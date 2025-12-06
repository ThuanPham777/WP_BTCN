using DrawingApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.Core.Interfaces.Repositories;

public interface IBoardRepository
{
    Task<List<DrawingBoard>> GetAllAsync();
    Task<DrawingBoard?> GetByIdAsync(Guid? id);
    Task AddAsync(DrawingBoard board);
    Task UpdateAsync(DrawingBoard board);
    Task DeleteAsync(Guid id);

    Task UpdateContentAsync(
        Guid boardId,
        string? name,
        double width,
        double height,
        string? background,
        List<BoardShape> newShapes);

}
