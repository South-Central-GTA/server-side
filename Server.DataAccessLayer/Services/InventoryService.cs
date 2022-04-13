using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Inventory;

namespace Server.DataAccessLayer.Services;

public class InventoryService
    : BaseService<InventoryModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public InventoryService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<InventoryModel?> GetByKey(int key)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Inventories
                              .Include(i => i.Items)
                              .ThenInclude(i => i.CatalogItemModel)
                              .Include(i => i.VehicleModel)
                              .Include(i => i.ItemClothModel)
                              .Include(i => i.HouseModel)
                              .FirstOrDefaultAsync(i => i.Id == key);
    }

    public override async Task<InventoryModel?> Find(Expression<Func<InventoryModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Inventories
                              .Include(i => i.Items)
                              .ThenInclude(i => i.CatalogItemModel)
                              .Include(i => i.VehicleModel)
                              .Include(i => i.ItemClothModel)
                              .Include(i => i.HouseModel)
                              .FirstOrDefaultAsync(expression);
    }
}