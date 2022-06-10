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
using Server.Database.Models;

namespace Server.DataAccessLayer.Services;

public class RoleplayInfoService : BaseService<RoleplayInfoModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public RoleplayInfoService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<RoleplayInfoModel?> GetByKey(int key)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.RoleplayInfos.Include(i => i.CharacterModel).FirstOrDefaultAsync(h => h.Id == key);
    }


    public override async Task<List<RoleplayInfoModel>> Where(Expression<Func<RoleplayInfoModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.RoleplayInfos.Include(i => i.CharacterModel).Where(expression).ToListAsync();
    }

    public override async Task<List<RoleplayInfoModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.RoleplayInfos.Include(i => i.CharacterModel).ToListAsync();
    }

    public async Task<RoleplayInfoModel> GetByDistance(Position position, float distance = 1.5f)
    {
        var houses = await GetAll();
        var closestDistance = float.MaxValue;
        RoleplayInfoModel closestInfoModel = null;
        foreach (var h in houses)
        {
            var distanceToHouse = new Position(h.PositionX, h.PositionY, h.PositionZ).Distance(position);
            if (distanceToHouse <= distance && distanceToHouse < closestDistance)
            {
                closestDistance = distanceToHouse;
                closestInfoModel = h;
            }
        }

        return closestInfoModel;
    }
}