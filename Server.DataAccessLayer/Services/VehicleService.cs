using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AltV.Net.Data;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Vehicles;

namespace Server.DataAccessLayer.Services;

public class VehicleService : BaseService<PlayerVehicleModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public VehicleService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PlayerVehicleModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Vehicles.Include(v => v.InventoryModel).ThenInclude(i => i.Items)
            .ThenInclude(i => i.CatalogItemModel).Include(v => v.GroupModelOwner).Include(v => v.CharacterModel)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public override async Task<List<PlayerVehicleModel>> Where(Expression<Func<PlayerVehicleModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Vehicles.Include(v => v.InventoryModel).ThenInclude(i => i.Items)
            .ThenInclude(i => i.CatalogItemModel).Include(v => v.GroupModelOwner).Include(v => v.CharacterModel)
            .Where(expression).ToListAsync();
    }

    public override async Task<List<PlayerVehicleModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Vehicles.Include(v => v.InventoryModel).ThenInclude(i => i.Items)
            .ThenInclude(i => i.CatalogItemModel).Include(v => v.GroupModelOwner).Include(v => v.CharacterModel)
            .ToListAsync();
    }

    public async Task<PlayerVehicleModel?> GetByDistance(Position position, float maxDistance = 1.5f)
    {
        var closestDistance = float.MaxValue;
        PlayerVehicleModel vehicleModel = null;
        foreach (var entity in await GetAll())
        {
            var distance = new Position(entity.PositionX, entity.PositionY, entity.PositionZ).Distance(position);
            if (distance <= maxDistance && distance < closestDistance)
            {
                closestDistance = distance;
                vehicleModel = entity;
            }
        }

        return vehicleModel;
    }
}