using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Inventory;
using Server.Modules.Character;
using Server.Modules.Group;
using Server.Modules.Houses;

namespace Server.Handlers.CharacterSelector;

public class RequestDeleteCharacterHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;
    private readonly HouseModule _houseModule;
    private readonly CharacterSelectionModule _characterSelectionModule;
    private readonly AccountService _accountService;
    private readonly BankAccountService _bankAccountService;
    private readonly CharacterService _characterService;
    private readonly DeliveryService _deliveryService;
    private readonly GroupMemberService _groupMemberService;
    private readonly GroupService _groupService;
    private readonly HouseService _houseService;
    private readonly InventoryService _inventoryService;
    private readonly ItemService _itemService;
    private readonly PublicGarageEntryService _publicGarageEntryService;
    private readonly RoleplayInfoService _roleplayInfoService;
    private readonly VehicleService _vehicleService;
    private readonly RegistrationOfficeService _registrationOfficeService;

    public RequestDeleteCharacterHandler(
        GroupModule groupModule,
        HouseModule houseModule,
        CharacterSelectionModule characterSelectionModule,
        AccountService accountService,
        BankAccountService bankAccountService,
        CharacterService characterService,
        DeliveryService deliveryService,
        GroupMemberService groupMemberService,
        GroupService groupService,
        HouseService houseService,
        InventoryService inventoryService,
        ItemService itemService,
        PublicGarageEntryService publicGarageEntryService,
        RoleplayInfoService roleplayInfoService,
        VehicleService vehicleService, RegistrationOfficeService registrationOfficeService)
    {
        _groupModule = groupModule;
        _houseModule = houseModule;
        _characterSelectionModule = characterSelectionModule;
        _accountService = accountService;
        _bankAccountService = bankAccountService;
        _characterService = characterService;
        _deliveryService = deliveryService;
        _groupMemberService = groupMemberService;
        _groupService = groupService;
        _houseService = houseService;
        _inventoryService = inventoryService;
        _itemService = itemService;
        _publicGarageEntryService = publicGarageEntryService;
        _roleplayInfoService = roleplayInfoService;
        _vehicleService = vehicleService;
        _registrationOfficeService = registrationOfficeService;

        AltAsync.OnClient<ServerPlayer, int>("charselector:requestdelete", OnRequestDeleteCharacter);
        AltAsync.OnClient<ServerPlayer, int>("charselector:delete", OnDeleteCharacter);
    }

    private async void OnRequestDeleteCharacter(ServerPlayer player, int characterId)
    {
        var data = new object[1];
        data[0] = characterId;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Charakter löschen",
            Description =
                "Dies ist unsere Debug Lösung Charaktere zu löschen.<br><br>Charakter wird gelöscht samt,<br>Fahrzeuge, Häuser, Bankkonten, eingeparkte Fahrzeuge in Public Garages, Unternehmen werden zufällig an den Ranghöhsten weitergegeben. Wenn kein Ranghöhster gefunden wurde bekommt irgendein Mitglied das Unternehmen, wenn kein Mitglied gefunden wurde dann wird das Unternehmen gelöscht und Häuser des Unternehmens werden verkauft.",
            PrimaryButton = "Löschen",
            Data = data,
            PrimaryButtonServerEvent = "charselector:delete"
        });
    }

    private async void OnDeleteCharacter(ServerPlayer player, int characterId)
    {
        if (!player.Exists)
        {
            return;
        }

        var character = await _characterService.GetByKey(characterId);
        if (character == null || character.AccountModelId != player.AccountModel.SocialClubId)
        {
            return;
        }

        if (player.AccountModel.LastSelectedCharacterId == characterId)
        {
            player.AccountModel.LastSelectedCharacterId = -1;
            await _accountService.Update(player.AccountModel);
        }

        var officeEntryModel = await _registrationOfficeService.GetByKey(characterId);
        if (officeEntryModel != null)
        {
            await _registrationOfficeService.Remove(officeEntryModel);
        }

        var ownedVehicles = await _vehicleService.Where(v => v.CharacterModelId == characterId);
        foreach (var ownedVehicle in ownedVehicles)
        {
            if (ownedVehicle.InventoryModel != null)
            {
                await _itemService.RemoveRange(ownedVehicle.InventoryModel.Items);
                await _inventoryService.Remove(ownedVehicle.InventoryModel);
            }

            var vehicle = Alt.GetAllVehicles().FindByDbId(ownedVehicle.Id);
            if (vehicle is not { Exists: true })
            {
                continue;
            }

            await vehicle.RemoveAsync();

            // Update possible open deliveries
            var deliveries = await _deliveryService.Where(d => d.PlayerVehicleModelId == ownedVehicle.Id);
            foreach (var delivery in deliveries)
            {
                delivery.PlayerVehicleModelId = null;
            }

            await _deliveryService.UpdateRange(deliveries);
        }

        await _vehicleService.RemoveRange(ownedVehicles);

        var ownedHouses = await _houseService.Where(h => h.CharacterModelId == characterId);
        foreach (var ownedHouse in ownedHouses)
        {
            // Dont reset house if it is used by the group.
            if (!ownedHouse.GroupModelId.HasValue)
            {
                if (ownedHouse.Inventory != null)
                {
                    await _itemService.RemoveRange(ownedHouse.Inventory.Items);
                    await _inventoryService.Remove(ownedHouse.Inventory);
                }

                await _houseModule.ResetOwner(ownedHouse);
            }
        }

        var groups = await _groupService.GetGroupsByCharacter(characterId);
        foreach (var group in groups)
        {
            var oldOwner = group.Members.Find(m => m.CharacterModelId == characterId);
            if (oldOwner != null)
            {
                group.Members.Remove(oldOwner);
                await _groupMemberService.Remove(oldOwner);
            }

            if (group.Members != null && group.Members.Count != 0)
            {
                var highestRank = group.Ranks.OrderByDescending(rank => rank.Level).First().Level;

                // Try get the highest member or if highest member is null, try find a random one.
                var highestMember = group.Members.Shuffle().FirstOrDefault(m => m.RankLevel == highestRank) ??
                                    group.Members.Shuffle().First();

                if (highestMember == null)
                {
                    continue;
                }

                highestMember.Owner = true;

                await _groupService.Update(group);
            }
            else
            {
                await _groupModule.DeleteGroup(group);
            }
        }

        var publicGarageEntries =
            await _publicGarageEntryService.Where(pg => pg.CharacterModelId == characterId);
        await _publicGarageEntryService.RemoveRange(publicGarageEntries);

        var ownedBankAccounts = await _bankAccountService.GetByOwner(characterId);
        await _bankAccountService.RemoveRange(ownedBankAccounts);

        if (character.InventoryModel != null)
        {
            foreach (var itemCloth in character.InventoryModel.Items.Where(i => i is ItemClothModel)
                                               .Cast<ItemClothModel>())
            {
                if (itemCloth.ClothingInventoryModel != null)
                {
                    await _itemService.RemoveRange(itemCloth.ClothingInventoryModel.Items);
                    await _inventoryService.Remove(itemCloth.ClothingInventoryModel);
                }
            }

            await _itemService.RemoveRange(character.InventoryModel.Items);
            await _inventoryService.Remove(character.InventoryModel);
        }

        var infos = await _roleplayInfoService.Where(i => i.CharacterModelId == characterId);
        await _roleplayInfoService.RemoveRange(infos);

        // We have to delete all character related tables first before deleting the character.

        await _characterService.Remove(character);

        await _characterSelectionModule.UpdateAsync(player);
    }
}