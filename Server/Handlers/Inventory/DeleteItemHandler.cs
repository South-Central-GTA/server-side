using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Modules.Inventory;

namespace Server.Handlers.Inventory;

public class DeleteItemHandler : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ItemDestructionModule _itemDestructionModule;

    public DeleteItemHandler(
        InventoryModule inventoryModule,
        ItemDestructionModule itemDestructionModule)
    {
        _inventoryModule = inventoryModule;
        _itemDestructionModule = itemDestructionModule;

        AltAsync.OnClient<ServerPlayer, int>("item:delete", OnDeleteItem);
    }

    private async void OnDeleteItem(ServerPlayer player, int itemId)
    {
        if (!player.Exists)
        {
            return;
        }
        
        await _itemDestructionModule.Destroy(itemId);

        player.SendNotification("Du hast das Item administrativ gelöscht.", NotificationType.INFO);

        await _inventoryModule.UpdateInventoryUiAsync(player);
        player.UpdateClothes();
    }
}