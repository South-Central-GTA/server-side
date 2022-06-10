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

public class OrderedVehicleService : BaseService<OrderedVehicleModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public OrderedVehicleService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<OrderedVehicleModel>> Where(Expression<Func<OrderedVehicleModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.OrderedVehicles.Include(ov => ov.CatalogVehicleModel).Where(expression).ToListAsync();
    }
}