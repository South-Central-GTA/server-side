using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Inventory;

namespace Server.DataAccessLayer.Services;

public class ItemKeyService
    : BaseService<ItemKeyModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public ItemKeyService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<ItemKeyModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemKeys
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .ToListAsync();
    }

    public async Task<ItemKeyModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemKeys
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .FirstOrDefaultAsync(i => i.Id == id);
    }

    public override async Task<ItemKeyModel?> Find(Expression<Func<ItemKeyModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemKeys
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .FirstOrDefaultAsync(expression);
    }
}