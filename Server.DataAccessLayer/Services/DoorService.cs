using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Housing;

namespace Server.DataAccessLayer.Services;

public class DoorService : BaseService<DoorModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public DoorService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<DoorModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Doors.Include(d => d.HouseModel).ToListAsync();
    }

    public async Task<DoorModel?> GetByDistance(Position position, float maxDistance = 1.5f)
    {
        var closestDistance = float.MaxValue;
        DoorModel doorModel = null;
        foreach (var h in await GetAll())
        {
            var distance = new Position(h.PositionX, h.PositionY, h.PositionZ).Distance(position);
            if (distance <= maxDistance && distance < closestDistance)
            {
                closestDistance = distance;
                doorModel = h;
            }
        }

        return doorModel;
    }
}