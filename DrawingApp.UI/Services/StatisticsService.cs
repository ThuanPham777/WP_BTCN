using DrawingApp.Core.Enums;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp.UI.Services;

public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _db;
    public StatisticsService(AppDbContext db) => _db = db;

    public async Task<Dictionary<ShapeType, int>> GetShapeUsageAsync()
    {
        return await _db.BoardShapes
            .AsNoTracking()
            .GroupBy(x => x.ShapeType)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }
}
