using System;
using System.Linq;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Vehicles;
using Server.Modules.Inventory;
using Server.Modules.Key;

namespace Server.Modules;

public class LockpickModule : ITransientScript
{
    private readonly Random _rand = new();
    private readonly LockModule _lockModule;
    private readonly ItemService _itemService;
    private readonly InventoryModule _inventoryModule;

    public LockpickModule(LockModule lockModule, ItemService itemService, InventoryModule inventoryModule)
    {
        _lockModule = lockModule;
        _itemService = itemService;
        _inventoryModule = inventoryModule;
    }

    public async Task PickLockAsync(ServerPlayer player, ILockableEntity lockableEntity)
    {
        if (lockableEntity.LockState == LockState.OPEN)
        {
            player.SendNotification("Das Schloss ist schon offen.", NotificationType.INFO);
            return;
        }
        
        if (_rand.NextDouble() > 0.4) // 60% chance to fail
        {
            await FailLockpick(player);
            return;
        }

        lockableEntity.LockState = LockState.OPEN;
        
        switch (lockableEntity)
        {
            case PlayerVehicleModel v:
                await _lockModule.HandleVehicleLock(v);
                break;
        }
        
        player.SendNotification("Das Schloss wurde aufgeknackt.", NotificationType.INFO);
    }

    private async Task FailLockpick(ServerPlayer player)
    {
        player.SendNotification("Der Dietrich ist abgebrochen und wurde zerstört.", NotificationType.ERROR);
        var item = player.CharacterModel.InventoryModel.Items.FirstOrDefault(i => i.CatalogItemModelId == ItemCatalogIds.LOCKPICK);
        if (item != null)
        {
            if (item.Amount >= 1)
            {
                item.Amount--;
                await _itemService.Update(item);

                return;
            }
                
            await _itemService.Remove(item);
        }
            
        await _inventoryModule.UpdateInventoryUiAsync(player);

    }
}