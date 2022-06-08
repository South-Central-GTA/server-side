using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Vehicles;

namespace Server.DataAccessLayer.Services;

public class PublicGarageEntryService
    : BaseService<PublicGarageEntryModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public PublicGarageEntryService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<PublicGarageEntryModel?> Find(Expression<Func<PublicGarageEntryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.PublicGarageEntries
                              .Include(g => g.GroupModel)
                              .Include(g => g.CharacterModel)
                              .Include(g => g.PlayerVehicleModel)
                              .FirstOrDefaultAsync(expression);
    }

    public override async Task<List<PublicGarageEntryModel>> Where(
        Expression<Func<PublicGarageEntryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.PublicGarageEntries
                              .Include(g => g.GroupModel)
                              .Include(g => g.CharacterModel)
                              .Include(g => g.PlayerVehicleModel)
                              .Where(expression)
                              .ToListAsync();
    }
}