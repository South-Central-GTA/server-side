using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Bank;
using Server.Modules.Group;
using Server.Modules.Inventory;

namespace Server.Handlers.Group;

public class CreateGroupKeyHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly InventoryModule _inventoryModule;
    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemService _itemService;

    public CreateGroupKeyHandler(
        BankAccountService bankAccountService,
        GroupService groupService,
        ItemCatalogService itemCatalogService,
        ItemService itemService,
        BankModule bankModule,
        GroupModule groupModule,
        ItemCreationModule itemCreationModule,
        InventoryModule inventoryModule)
    {
        _bankAccountService = bankAccountService;
        _groupService = groupService;
        _itemCatalogService = itemCatalogService;
        _itemService = itemService;

        _bankModule = bankModule;
        _groupModule = groupModule;
        _itemCreationModule = itemCreationModule;
        _inventoryModule = inventoryModule;

        AltAsync.OnClient<ServerPlayer, int>("group:creategroupkey", OnCreateGroupKey);
    }

    private async void OnCreateGroupKey(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.GROUP_KEY))
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer von dieser Gruppe.", NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByOwningGroup(group.Id);
        if (bankAccount == null)
        {
            player.SendNotification("Es konnte kein Bankkonto von der Gruppe gefunden werden.", NotificationType.ERROR);
            return;
        }

        var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.GROUP_KEY);

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            player.SendNotification($"Dein Charakter hat keine Transferrechte für das Konto {bankAccount.BankDetails}.", NotificationType.ERROR);
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount, catalogItem.Price, false, "Gruppenschlüssel nachgemacht");
        if (success)
        {
            var item = (ItemGroupKeyModel)await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.GROUP_KEY, 1, null, null, group.Name);
            if (item == null)
            {
                return;
            }

            item.GroupModelId = group.Id;

            await _itemService.Update(item);

            player.SendNotification("Du hast erfolgreich einen weiteren Gruppenschlüssel erstellt. Dieser ist nur für Gruppenmitglieder verwendbar.", NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification("Das Gruppenkonto hat nicht genug Geld um einen Gruppenschlüssel zu erstellen.", NotificationType.ERROR);
        }
    }
}