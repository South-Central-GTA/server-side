using System;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Inventory;

namespace Server.Modules.PoliceTicket;

public class PoliceTicketModule : ISingletonScript
{
    private readonly PoliceTicketService _policeTicketService;
    private readonly ItemService _itemService;
    private readonly GroupFactionService _groupFactionService;
    private readonly BankAccountService _bankAccountService;

    private readonly ItemCreationModule _itemCreationModule;
    private readonly InventoryModule _inventoryModule;

    public PoliceTicketModule(
        PoliceTicketService policeTicketService,
        ItemService itemService,
        GroupFactionService groupFactionService,
        BankAccountService bankAccountService,
        ItemCreationModule itemCreationModule,
        InventoryModule inventoryModule)
    {
        _policeTicketService = policeTicketService;
        _itemService = itemService;
        _groupFactionService = groupFactionService;
        _bankAccountService = bankAccountService;

        _itemCreationModule = itemCreationModule;
        _inventoryModule = inventoryModule;
    }

    public async Task<bool> GivePlayerTicket(ServerPlayer player, string creatorCharacterName, string reason, int price)
    {
        if (await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.POLICE_TICKET, 1) is not ItemPoliceTicketModel
            item)
        {
            return false;
        }

        item.ReferenceId = await GetRandomReferenceId();
        item.Costs = price;
        item.Reason = reason;
        item.CreatorCharacterName = creatorCharacterName;
        item.TargetCharacterId = player.CharacterModel.Id;

        await _itemService.Update(item);

        await _inventoryModule.UpdateInventoryUiAsync(player);
        player.SendNotification("Dein Charakter hat ein Ticket erhalten.", NotificationType.INFO);

        return true;
    }

    public async Task<string> GetRandomReferenceId()
    {
        var rnd = new Random();
        var number = rnd.Next(10000000, 99999999);

        while (await GetByReferenceId(number.ToString()) != null)
        {
            number = rnd.Next(10000000, 99999999);
        }

        return number.ToString();
    }

    public async Task<ItemPoliceTicketModel?> GetByReferenceId(string referenceId)
    {
        return await _policeTicketService.Find(p => p.ReferenceId == referenceId);
    }

    public async Task CheckPoliceTickets(ServerPlayer player, string receiverBankAccountDetails, string useOfPurpose,
                                         int value)
    {
        var pdFaction = await _groupFactionService.Find(gf => gf.FactionType == FactionType.POLICE_DEPARTMENT);
        if (pdFaction == null)
        {
            return;
        }

        var pdBankAccount = await _bankAccountService.GetByGroup(pdFaction.Id);
        if (pdBankAccount == null)
        {
            return;
        }

        if (pdBankAccount.BankDetails != receiverBankAccountDetails)
        {
            return;
        }

        var policeTicket = await _policeTicketService.Find(pt => pt.ReferenceId == useOfPurpose.Trim());
        if (policeTicket == null)
        {
            return;
        }

        if (policeTicket.Costs != value)
        {
            return;
        }

        if (policeTicket.Payed)
        {
            player.SendNotification("Dieser Strafzettel wurde schon bezahlt.", NotificationType.SUCCESS);
            return;
        }

        policeTicket.Payed = true;

        player.SendNotification("Ein Strafzettel wurde bezahlt.", NotificationType.SUCCESS);

        await _policeTicketService.Update(policeTicket);
    }
}