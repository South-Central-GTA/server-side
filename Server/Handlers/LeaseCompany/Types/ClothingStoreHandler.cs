using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Handlers.LeaseCompany.Types.Base;
using Server.Helper;
using Server.Modules.Bank;
using Server.Modules.Chat;
using Server.Modules.Clothing;
using Server.Modules.Inventory;
using Server.Modules.Money;

namespace Server.Handlers.LeaseCompany.Types;

public class ClothingStoreHandler
    : BaseItemShopHandler
{
    private readonly HouseService _houseService;

    private readonly InventoryModule _inventoryModule;

    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly Serializer _serializer;
    private readonly UserShopDataService _userShopDataService;

    public ClothingStoreHandler(
        IOptions<CompanyOptions> companyOptions,
        Serializer serializer,
        ItemCatalogService itemCatalogService,
        HouseService houseService,
        BankAccountService bankAccountService,
        ItemService itemService,
        GroupService groupService,
        UserShopDataService userShopDataService,
        InventoryService inventoryService,
        ChatModule chatModule,
        InventoryModule inventoryModule,
        ItemCreationModule itemCreationModule,
        MoneyModule moneyModule,
        BankModule bankModule) : base(companyOptions, 
                                      itemCatalogService,
                                      houseService,
                                      bankAccountService,
                                      itemService,
                                      groupService,
                                      userShopDataService,
                                      inventoryService,
                                      inventoryModule,
                                      itemCreationModule,
                                      moneyModule,
                                      bankModule)
    {
        _serializer = serializer;

        _itemCatalogService = itemCatalogService;
        _houseService = houseService;
        _userShopDataService = userShopDataService;

        _inventoryModule = inventoryModule;
        _itemCreationModule = itemCreationModule;

        AltAsync.OnClient<ServerPlayer>("clothingstore:requeststartchangeclothes", OnRequestStartChangeClothes);
        AltAsync.OnClient<ServerPlayer>("clothingstore:cancel", OnCancel);
        AltAsync.OnClient<ServerPlayer, string>("clothingstore:requestitems", OnRequestItems);
        AltAsync.OnClient<ServerPlayer>("clothingstore:requestopencashiermenu", OnRequestOpenCashierMenu);
        AltAsync.OnClient<ServerPlayer, int>("clothingstore:buywithcash", OnBuyWithCash);
        AltAsync.OnClient<ServerPlayer, int>("clothingstore:buywithbank", OnBuyWithBank);
        AltAsync.OnClient<ServerPlayer>("clothingstore:requestreturnitems", OnRequestReturnItems);
    }

    private async void OnRequestStartChangeClothes(ServerPlayer player)
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

        if (!leaseCompanyHouse.HasOpen && !leaseCompanyHouse.PlayerDuty)
        {
            player.SendNotification("Dieser Laden hat geschlossen.", NotificationType.ERROR);
            return;
        }

        await AltAsync.Do(() =>
        {
            player.EmitLocked("clothingstore:startchangeclothes", player.CharacterModel);
            player.SetUniqueDimension();
        });
    }

    private async void OnCancel(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await player.SetDimensionAsync(0);
    }

    private async void OnRequestItems(ServerPlayer player, string clothingsJson)
    {
        if (!player.Exists)
        {
            return;
        }

        await player.SetDimensionAsync(0);

        var clothingsData = _serializer.Deserialize<ClothingsData>(clothingsJson);
        float weight = 0;

        if (clothingsData.Hat != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_HAT);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Glasses != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_GLASSES);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Ears != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_EARS);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Watch != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_WATCH);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Bracelets != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_BRACELET);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Mask != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_MASK);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Top != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_TOP);
            weight += catalogItem.Weight;
        }

        if (clothingsData.BodyArmor != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_BODY_ARMOR);
            weight += catalogItem.Weight;
        }

        if (clothingsData.BackPack != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_BACKPACK);
            weight += catalogItem.Weight;
        }

        if (clothingsData.UnderShirt != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_UNDERSHIRT);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Accessories != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_ACCESSORIES);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Pants != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_PANTS);
            weight += catalogItem.Weight;
        }

        if (clothingsData.Shoes != null)
        {
            var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.CLOTHING_SHOES);
            weight += catalogItem.Weight;
        }

        if (!await _inventoryModule.CanCarry(player, player.CharacterModel.InventoryModel.Id, weight))
        {
            return;
        }

        var didAllItemsFit = true;

        if (clothingsData.Hat != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_HAT, 1, null, _serializer.Serialize(clothingsData.Hat), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_HAT, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Glasses != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_GLASSES, 1, null, _serializer.Serialize(clothingsData.Glasses), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_GLASSES, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Ears != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_EARS, 1, null, _serializer.Serialize(clothingsData.Ears), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_EARS, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Watch != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_WATCH, 1, null, _serializer.Serialize(clothingsData.Watch), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_WATCH, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Bracelets != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_BRACELET, 1, null, _serializer.Serialize(clothingsData.Bracelets), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_BRACELET, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Mask != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_MASK, 1, null, _serializer.Serialize(clothingsData.Mask), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_MASK, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Top != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_TOP, 1, null, _serializer.Serialize(clothingsData.Top), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_TOP, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.BodyArmor != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_BODY_ARMOR, 1, null, _serializer.Serialize(clothingsData.BodyArmor), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_BODY_ARMOR, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.BackPack != null)
        {
            if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.CLOTHING_BACKPACK))
            {
                return;
            }

            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_BACKPACK, 1, null, _serializer.Serialize(clothingsData.BackPack), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_BACKPACK, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.UnderShirt != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_UNDERSHIRT, 1, null, _serializer.Serialize(clothingsData.UnderShirt), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_UNDERSHIRT, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Accessories != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_ACCESSORIES, 1, null, _serializer.Serialize(clothingsData.Accessories), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_ACCESSORIES, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Pants != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_PANTS, 1, null, _serializer.Serialize(clothingsData.Pants), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_PANTS, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (clothingsData.Shoes != null)
        {
            var item = await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.CLOTHING_SHOES, 1, null, _serializer.Serialize(clothingsData.Shoes), null, true, false);
            if (item != null)
            {
                await SetUnboughtItems(player, ItemCatalogIds.CLOTHING_SHOES, 1);
            }
            else
            {
                didAllItemsFit = false;
            }
        }

        if (didAllItemsFit)
        {
            player.SendNotification("Alle Kleidungsstücke wurde dem Warenkorb hinzugefügt.", NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification("Mindestens ein Kleidungsstück hat nicht mehr in das Inventar gepasst.", NotificationType.WARNING);
        }

        await _inventoryModule.UpdateInventoryUiAsync(player);
    }

    private async void OnRequestOpenCashierMenu(ServerPlayer player)
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

        if (!leaseCompanyHouse.HasOpen && !leaseCompanyHouse.PlayerDuty)
        {
            player.SendNotification("Dieser Laden hat geschlossen.", NotificationType.ERROR);
            return;
        }

        var costs = await GetBill(player);
        if (costs == 0)
        {
            player.SendNotification("Dein Charakter hat keine Waren von dem Kleidungsladen im Inventar.", NotificationType.ERROR);
            return;
        }

        player.CreateDialog(new DialogData
        {
            Type = DialogType.TWO_BUTTON_DIALOG,
            Title = "Kleidungsladen",
            Description = $"Deine ausgewählten Kleidungsstücke kosten <b>${costs}</b>, willst du diese bezahlen?<br><p class='text-muted'>Du kannst mit dem Bargeld deines Charakters bezahlen oder per Banküberweisung.</p>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            PrimaryButton = "Bargeld nutzen",
            PrimaryButtonServerEvent = "clothingstore:buywithcash",
            SecondaryButton = "Karte nutzen",
            SecondaryButtonServerEvent = "clothingstore:buywithbank"
        });
    }

    protected override async Task<int> GetBill(ServerPlayer player)
    {
        var price = 0;

        foreach (var item in player.CharacterModel.InventoryModel.Items
                                   .Where(i => !i.IsBought && ClothingModule.IsClothesOrProp(i.CatalogItemModelId)))
        {
            var componentId = ClothingModule.GetComponentId(item.CatalogItemModelId);
            if (!componentId.HasValue)
            {
                continue;
            }

            var dbItem = await _itemCatalogService.GetByKey(item.CatalogItemModelId);
            price += dbItem.Price;
        }

        return price;
    }

    protected override async Task SetUnboughtItems(ServerPlayer player, ItemCatalogIds catalogItemId, int amount)
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
            await _userShopDataService.Add(new UserShopDataModel { CharacterModelId = player.CharacterModel.Id, GotWarned = false, BillToPay = price });
        }
    }
}