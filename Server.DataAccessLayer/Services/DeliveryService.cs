using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Delivery;

namespace Server.DataAccessLayer.Services;

public class DeliveryService
    : BaseService<DeliveryModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public DeliveryService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DeliveryModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Deliveries
                              .Include(d => d.OrderGroupModel)
                              .Include(d => d.SupplierGroupModel)
                              .Include(d => d.PlayerVehicleModel)
                              .FirstOrDefaultAsync(i => i.Id == id);
    }

    public override async Task<DeliveryModel?> Find(Expression<Func<DeliveryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Deliveries
                              .Include(d => d.OrderGroupModel)
                              .Include(d => d.SupplierGroupModel)
                              .Include(d => d.PlayerVehicleModel)
                              .FirstOrDefaultAsync(expression);
    }

    public override async Task<List<DeliveryModel>> Where(Expression<Func<DeliveryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Deliveries
                              .Include(d => d.OrderGroupModel)
                              .Include(d => d.SupplierGroupModel)
                              .Include(d => d.PlayerVehicleModel)
                              .Where(expression)
                              .ToListAsync();
    }
}