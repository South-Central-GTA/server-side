using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Modules.Inventory;

namespace Server.Handlers.Group;

public class GroupMemberInventoryHandler : ISingletonScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly InventoryService _inventoryService;

    public GroupMemberInventoryHandler(
        InventoryService inventoryService,
        InventoryModule inventoryModule)
    {
        _inventoryService = inventoryService;
        _inventoryModule = inventoryModule;

        AltAsync.OnClient<ServerPlayer, int, int>("group:openothersgroupinventory", OnOpenOtherGroupInventory);
    }

    private async void OnOpenOtherGroupInventory(ServerPlayer player, int groupId, int characterId)
    {
        var inventory = await _inventoryService.Find(i =>
                                                         i.GroupCharacterId == characterId && i.GroupId == groupId);
        if (inventory == null)
        {
            return;
        }

        player.OpenInventories = new List<OpenInventoryData> { new(inventory.InventoryType, inventory.Id) };

        await _inventoryModule.OpenInventoryUiAsync(player);
    }
}