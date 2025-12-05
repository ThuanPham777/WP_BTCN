using DrawingApp.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingApp.Core.Interfaces.Services;

public interface IStatisticsService
{
    Task<Dictionary<ShapeType, int>> GetShapeUsageAsync();
}
