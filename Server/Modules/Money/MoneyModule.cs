using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Inventory;

namespace Server.Modules.Money;

public class MoneyModule : ITransientScript
{
    private readonly InventoryModule _inventoryModule;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemService _itemService;
    private readonly ILogger<MoneyModule> _logger;

    public MoneyModule(ILogger<MoneyModule> logger, ItemService itemService, InventoryModule inventoryModule,
        ItemCreationModule itemCreationModule)
    {
        _logger = logger;
        _itemService = itemService;

        _inventoryModule = inventoryModule;
        _itemCreationModule = itemCreationModule;
    }

    public async Task<bool> WithdrawAsync(ServerPlayer player, int amount)
    {
        var removeItems = new List<ItemModel>();
        var items = player.CharacterModel.InventoryModel.Items;
        var successfull = false;
        var amountToPay = amount;

        foreach (var item in items)
        {
            if (item.CatalogItemModel.Id == ItemCatalogIds.DOLLAR)
            {
                var itemAmount = item.Amount;

                item.Amount -= amountToPay;
                amountToPay -= itemAmount;

                if (item.Amount <= 0)
                {
                    removeItems.Add(item);
                }

                if (amountToPay <= 0)
                {
                    break;
                }
            }
        }

        if (amountToPay <= 0)
        {
            await _itemService.RemoveRange(removeItems);

            items.RemoveAll(i => removeItems.Contains(i));

            await _itemService.UpdateRange(items);
            successfull = true;
        }
        else
        {
            successfull = false;
        }

        await _inventoryModule.UpdateInventoryUiAsync(player);
        return successfull;
    }

    public async Task<bool> GiveMoney(ServerPlayer player, int value)
    {
        var existingMoneyItem =
            player.CharacterModel.InventoryModel.Items.FirstOrDefault(i => i.CatalogItemModelId == ItemCatalogIds.DOLLAR);
        if (existingMoneyItem != null)
        {
            existingMoneyItem.Amount += value;
            await _itemService.Update(existingMoneyItem);
            await _inventoryModule.UpdateInventoryUiAsync(player);
        }
        else
        {
            var moneyItem = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.DOLLAR, value);
            if (moneyItem == null)
            {
                return false;
            }
        }

        return true;
    }
}