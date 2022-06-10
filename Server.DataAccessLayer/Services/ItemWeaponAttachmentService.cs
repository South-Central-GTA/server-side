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

public class ItemWeaponAttachmentService : BaseService<ItemWeaponAttachmentModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public ItemWeaponAttachmentService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<List<ItemWeaponAttachmentModel>> GetAll()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemWeaponAttachments.Include(i => i.CatalogItemModel).Include(i => i.InventoryModel)
            .ThenInclude(i => i.CharacterModel).ToListAsync();
    }

    public async Task<ItemWeaponAttachmentModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemWeaponAttachments.Include(i => i.CatalogItemModel).Include(i => i.InventoryModel)
            .ThenInclude(i => i.CharacterModel).FirstOrDefaultAsync(i => i.Id == id);
    }

    public override async Task<ItemWeaponAttachmentModel?> Find(
        Expression<Func<ItemWeaponAttachmentModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemWeaponAttachments.Include(i => i.CatalogItemModel).Include(i => i.InventoryModel)
            .ThenInclude(i => i.CharacterModel).FirstOrDefaultAsync(expression);
    }

    public override async Task<List<ItemWeaponAttachmentModel>> Where(
        Expression<Func<ItemWeaponAttachmentModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemWeaponAttachments.Include(i => i.CatalogItemModel).Include(i => i.InventoryModel)
            .ThenInclude(i => i.CharacterModel).Where(expression).ToListAsync();
    }
}