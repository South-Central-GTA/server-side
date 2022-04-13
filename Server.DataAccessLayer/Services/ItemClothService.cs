using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Inventory;

namespace Server.DataAccessLayer.Services;

public class ItemClothService
    : BaseService<ItemClothModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public ItemClothService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<ItemClothModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemCloths
                              .Include(i => i.ClothingInventoryModel)
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .ToListAsync();
    }

    public async Task<ItemClothModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemCloths
                              .Include(i => i.ClothingInventoryModel)
                              .ThenInclude(i => i.Items)
                              .ThenInclude(i => i.CatalogItemModel)
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .FirstOrDefaultAsync(i => i.Id == id);
    }

    public override async Task<ItemClothModel?> Find(Expression<Func<ItemClothModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemCloths
                              .Include(i => i.ClothingInventoryModel)
                              .ThenInclude(i => i.Items)
                              .ThenInclude(i => i.CatalogItemModel)
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .FirstOrDefaultAsync(expression);
    }

    public override async Task<List<ItemClothModel>> Where(Expression<Func<ItemClothModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemCloths
                              .Include(i => i.ClothingInventoryModel)
                              .ThenInclude(i => i.Items)
                              .ThenInclude(i => i.CatalogItemModel)
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .Where(expression).ToListAsync();
    }
}