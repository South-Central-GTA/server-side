using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Database.Models.Inventory;
using Server.Modules.Bank;
using Server.Modules.Clothing;
using Server.Modules.Inventory;
using Server.Modules.Money;

namespace Server.Handlers.LeaseCompany.Types.Base;

public class BaseItemShopHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly CompanyOptions _companyOptions;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;

    private readonly InventoryModule _inventoryModule;
    private readonly InventoryService _inventoryService;
    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemService _itemService;
    private readonly MoneyModule _moneyModule;
    private readonly UserShopDataService _userShopDataService;

    public BaseItemShopHandler(IOptions<CompanyOptions> companyOptions, ItemCatalogService itemCatalogService,
        HouseService houseService, BankAccountService bankAccountService, ItemService itemService,
        GroupService groupService, UserShopDataService userShopDataService, InventoryService inventoryService,
        InventoryModule inventoryModule, MoneyModule moneyModule,
        BankModule bankModule)
    {
        _companyOptions = companyOptions.Value;
        _itemCatalogService = itemCatalogService;
        _houseService = houseService;
        _bankAccountService = bankAccountService;
        _itemService = itemService;
        _groupService = groupService;
        _userShopDataService = userShopDataService;
        _inventoryService = inventoryService;

        _inventoryModule = inventoryModule;
        _moneyModule = moneyModule;
        _bankModule = bankModule;
    }

    protected async void OnBuyWithBank(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du nicht einkaufen.", NotificationType.ERROR);
            return;
        }

        var costs = await GetBill(player);
        if (costs == 0)
        {
            player.SendNotification("Dein Charakter hat keine Waren von dem Supermarkt im Inventar.",
                NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            player.SendNotification($"Dein Charakter hat keine Transferrechte für das Konto {bankAccount.BankDetails}.",
                NotificationType.ERROR);
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount, costs, false,
            $"{leaseCompanyHouse.SubName} {_companyOptions.Types[leaseCompanyHouse.LeaseCompanyType].Name}");
        if (success)
        {
            player.SendNotification("Dein Charakter hat den Einkauf bezahlt.", NotificationType.SUCCESS);
            await RemovePlayerFromData(player, false);

            if (leaseCompanyHouse.GroupModelId.HasValue)
            {
                var owningGroup = await _groupService.GetByKey(leaseCompanyHouse.GroupModelId);
                if (owningGroup != null)
                {
                    var owningGroupBankAccount = await _bankAccountService.GetByOwningGroup(owningGroup.Id);
                    await _bankModule.Deposit(owningGroupBankAccount, costs, "Verkauf von Ware");
                }
            }
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Geld auf dem Bankkonto für diesen Einkauf.",
                NotificationType.ERROR);
        }
    }

    protected virtual async void OnRequestReturnItems(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            return;
        }

        if (player.CharacterModel.InventoryModel.Items.Count(i => !i.IsBought) != 0)
        {
            var unboughtItems = player.CharacterModel.InventoryModel.Items.FindAll(i => !i.IsBought);

            foreach (var item in unboughtItems.Where(item => item is ItemClothModel))
            {
                var inv = await _inventoryService.Find(i => i.ItemClothModelId == item.Id);
                if (inv != null)
                {
                    await _inventoryService.Remove(inv);
                }
            }

            await _itemService.RemoveRange(unboughtItems);
            await _inventoryModule.UpdateInventoryUiAsync(player);

            var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
            if (shopData != null)
            {
                await _userShopDataService.Remove(shopData);
            }

            player.SendNotification("Dein Charakter hat die Waren zurückgegeben.", NotificationType.INFO);
        }
        else
        {
            player.SendNotification("Dein Charakter hat keine Waren zum Zurückgeben.", NotificationType.ERROR);
        }
    }

    protected async void OnBuyWithCash(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (await _houseService.GetByDistance(player.Position, 20) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            player.SendNotification("Hier kannst du nicht einkaufen.", NotificationType.ERROR);
            return;
        }

        var costs = await GetBill(player);
        if (costs == 0)
        {
            player.SendNotification("Dein Charakter hat keine offene Rechnung.", NotificationType.ERROR);
            return;
        }

        var success = await _moneyModule.WithdrawAsync(player, costs);
        if (success)
        {
            player.SendNotification("Erfolgreich bezahlt.", NotificationType.SUCCESS);
            await RemovePlayerFromData(player, false);

            if (leaseCompanyHouse.GroupModelId.HasValue)
            {
                var owningGroup = await _groupService.GetByKey(leaseCompanyHouse.GroupModelId);
                if (owningGroup != null)
                {
                    var owningGroupBankAccount = await _bankAccountService.GetByOwningGroup(owningGroup.Id);
                    await _bankModule.Deposit(owningGroupBankAccount, costs, "Verkauf von Ware");
                }
            }
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Bargeld.", NotificationType.ERROR);
        }
    }

    protected virtual async Task<int> GetBill(ServerPlayer player)
    {
        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        return shopData?.BillToPay ?? 0;
    }

    protected virtual async Task SetUnboughtItems(ServerPlayer player, ItemCatalogIds catalogItemId, int amount)
    {
        var catalogItem = await _itemCatalogService.GetByKey(catalogItemId);
        var price = catalogItem.Price * amount;

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData != null)
        {
            shopData.BillToPay += price;
            await _userShopDataService.Update(shopData);
        }
        else
        {
            await _userShopDataService.Add(new UserShopDataModel
            {
                CharacterModelId = player.CharacterModel.Id, GotWarned = false, BillToPay = price
            });
        }
    }

    private async Task RemovePlayerFromData(ServerPlayer player, bool stoleItems)
    {
        player.CharacterModel.InventoryModel.Items.ForEach(i =>
        {
            if (!i.IsBought)
            {
                i.IsBought = true;
                i.IsStolen = stoleItems;
            }
        });

        await _itemService.UpdateRange(player.CharacterModel.InventoryModel.Items);

        var shopData = await _userShopDataService.GetByKey(player.CharacterModel.Id);
        if (shopData != null)
        {
            await _userShopDataService.Remove(shopData);
        }
    }
}