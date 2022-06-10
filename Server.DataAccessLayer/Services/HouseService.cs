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
using Server.Database.Models.Housing;

namespace Server.DataAccessLayer.Services;

public class HouseService : BaseService<HouseModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public HouseService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<HouseModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Houses.Include(h => h.Doors).Include(h => h.Inventory).ThenInclude(i => i.Items)
            .ThenInclude(i => i.CatalogItemModel).ToListAsync();
    }

    public async Task<HouseModel?> GetByKey(int key)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Houses.Include(h => h.Doors).Include(h => h.Inventory).ThenInclude(i => i.Items)
            .ThenInclude(i => i.CatalogItemModel).FirstOrDefaultAsync(h => h.Id == key);
    }


    public override async Task<List<HouseModel>> Where(Expression<Func<HouseModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Houses.Include(h => h.Doors).Include(h => h.Inventory).ThenInclude(i => i.Items)
            .ThenInclude(i => i.CatalogItemModel).Where(expression).ToListAsync();
    }

    public async Task<HouseModel?> GetByDistance(Position position, float maxDistance = 1.5f)
    {
        var closestDistance = float.MaxValue;
        HouseModel houseModel = null;
        foreach (var h in await GetAll())
        {
            var distance = new Position(h.PositionX, h.PositionY, h.PositionZ).Distance(position);
            if (distance <= maxDistance && distance < closestDistance)
            {
                closestDistance = distance;
                houseModel = h;
            }
        }

        return houseModel;
    }
}