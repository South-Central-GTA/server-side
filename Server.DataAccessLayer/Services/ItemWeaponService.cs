using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models;
using Server.Database.Models.Inventory;

namespace Server.DataAccessLayer.Services;

public class ItemWeaponService
    : BaseService<ItemWeaponModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public ItemWeaponService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<ItemWeaponModel?> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ItemWeapons
                              .Include(i => i.CatalogItemModel)
                              .Include(i => i.InventoryModel)
                              .ThenInclude(i => i.CharacterModel)
                              .FirstOrDefaultAsync(i => i.Id == id);
    }
}