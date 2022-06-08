using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging;
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
using Server.Modules.EntitySync;
using Server.Modules.Inventory;
using Server.Modules.SouthCentralPoints;

namespace Server.Modules.Houses;

public class HouseModule : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;

    private readonly BankModule _bankModule;
    private readonly CharacterCreatorOptions _characterCreatorOptions;
    private readonly GroupService _groupService;
    private readonly DoorService _doorService;

    private readonly HouseService _houseService;
    private readonly InventoryModule _inventoryModule;
    private readonly InventoryService _inventoryService;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemService _itemService;

    private readonly PedSyncModule _pedSyncModule;
    private readonly DoorSyncModule _doorSyncModule;

    private readonly Dictionary<int, ServerPlayer> _playerSelections = new();
    private readonly SouthCentralPointsModule _southCentralPointsModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public HouseModule(
        ILogger<HouseModule> logger,
        IOptions<WorldLocationOptions> worldLocationOptions,
        IOptions<CharacterCreatorOptions> characterCreatorOptions,
        HouseService houseService,
        BankAccountService bankAccountService,
        ItemService itemService,
        InventoryService inventoryService,
        GroupService groupService,
        BankModule bankModule,
        InventoryModule inventoryModule,
        SouthCentralPointsModule southCentralPointsModule,
        PedSyncModule pedSyncModule,
        ItemCreationModule itemCreationModule,
        DoorSyncModule doorSyncModule,
        DoorService doorService)
    {
        _worldLocationOptions = worldLocationOptions.Value;
        _characterCreatorOptions = characterCreatorOptions.Value;

        _houseService = houseService;
        _bankAccountService = bankAccountService;
        _itemService = itemService;
        _inventoryService = inventoryService;
        _groupService = groupService;
        _bankModule = bankModule;
        _inventoryModule = inventoryModule;
        _southCentralPointsModule = southCentralPointsModule;
        _pedSyncModule = pedSyncModule;
        _itemCreationModule = itemCreationModule;
        _doorSyncModule = doorSyncModule;
        _doorService = doorService;
    }

    public Dictionary<int, IColShape> ColShapes { get; } = new();

    public async Task CreateCollShapes()
    {
        var houses = await _houseService.GetAll();
        foreach (var house in houses)
        {
            ColShapes.Add(house.Id,
                          Alt.CreateColShapeSphere(new Position(house.PositionX, house.PositionY, house.PositionZ),
                                                   1.5f));
        }
    }

    public async Task CreateInventories()
    {
        var houses = await _houseService.GetAll();
        var newInventories = new List<InventoryModel>();

        foreach (var house in houses.Where(h => h.Inventory == null))
        {
            newInventories.Add(new InventoryModel
            {
                HouseModelId = house.Id,
                Name = "Hauslager",
                InventoryType = InventoryType.HOUSE,
                MaxWeight = 100
            });
        }

        await _inventoryService.AddRange(newInventories);
    }

    public async Task BuyHouse(ServerPlayer player, HouseModel houseModel, int bankAccountId)
    {
        if (houseModel.HasOwner)
        {
            player.SendNotification("Dieses Haus hat schon einen Eigentümer.", NotificationType.ERROR);
            return;
        }

        if (IsHouseBlocked(houseModel.Id))
        {
            player.SendNotification(
                "Dieses Haus wurde sich in der Charaktererstellung vorgemerkt und kann aktuell nicht gekauft werden.",
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

        var success = await _bankModule.Withdraw(bankAccount, houseModel.Price, false, "Immobilienkauf");
        if (!success)
        {
            player.SendNotification($"Auf dem Konto {bankAccount.BankDetails} liegt nicht genügend Geld.",
                                    NotificationType.ERROR);
            return;
        }

        if (IsStarterHouse(houseModel))
        {
            await UpdateHouses();
        }

        await SetOwner(player, houseModel);

        player.SendNotification($"Dein Charakter hat sich das Haus für ${houseModel.Price} gekauft.",
                                NotificationType.SUCCESS);
    }

    public async Task SetOwner(ServerPlayer player, HouseModel houseModel)
    {
        houseModel.LockState = LockState.CLOSED;

        if (!await CreateHouseKey(player, houseModel))
        {
            return;
        }

        houseModel.CharacterModelId = player.CharacterModel.Id;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task<bool> CreateHouseKey(ServerPlayer player, HouseModel houseModel)
    {
        var itemKey =
            (ItemKeyModel)await _itemCreationModule.AddItemAsync(player,
                                                                 ItemCatalogIds.KEY,
                                                                 1,
                                                                 null,
                                                                 null,
                                                                 "Immobilienschlüssel");
        if (itemKey == null)
        {
            return false;
        }

        houseModel.Keys ??= new List<int>();

        houseModel.Keys.Add(itemKey.Id);

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);

        await _inventoryModule.UpdateInventoryUiAsync(player);

        return true;
    }

    public async Task Sell(ServerPlayer player, HouseModel houseModel, int bankAccountId)
    {
        if (houseModel.CharacterModelId != player.CharacterModel.Id)
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer dieses Hauses.", NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        var price = (int)(houseModel.Price * 0.8f);

        await _bankModule.Deposit(bankAccount, price, "Immobilienverkauf");
        player.SendNotification(
            $"Dein Charakter hat Haus verkauft und ${price} auf das Bankkonto {bankAccount.BankDetails} überwiesen bekommen.",
            NotificationType.SUCCESS);

        await ResetOwner(houseModel);

        var keyItems = player.CharacterModel.InventoryModel.Items.FindAll(
            i => i.CatalogItemModelId == ItemCatalogIds.KEY
                 && houseModel.Keys.Any(k => k == i.Id));

        await _itemService.RemoveRange(keyItems);
        await _inventoryModule.UpdateInventoryUiAsync(player);
    }

    public async Task ResetOwner(HouseModel houseModel)
    {
        houseModel.CharacterModelId = null;
        houseModel.GroupModelId = null;
        houseModel.Keys = new List<int>();
        houseModel.LockState = LockState.CLOSED;
        houseModel.RentBankAccountId = null;

        if (houseModel.Inventory != null)
        {
            await _itemService.RemoveRange(houseModel.Inventory.Items);
        }

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task ResetOwners(List<HouseModel> houses)
    {
        foreach (var house in houses)
        {
            house.CharacterModelId = null;
            house.GroupModelId = null;
            house.Keys = new List<int>();
            house.LockState = LockState.CLOSED;

            foreach (var houseDoor in house.Doors)
            {
                houseDoor.LockState = LockState.CLOSED;
            }

            await UpdateOnClient(house);
        }

        await _houseService.UpdateRange(houses);
    }

    public async Task CreateHouse(Position position, Rotation rotation, int interiorId, int houseNumber, int price,
                                  string subName, int streetDirection)
    {
        if (subName == "FREI")
        {
            subName = "";
        }

        var house = new HouseModel(position.X,
                                   position.Y,
                                   position.Z,
                                   rotation.Roll,
                                   rotation.Pitch,
                                   rotation.Yaw,
                                   interiorId,
                                   houseNumber,
                                   price,
                                   subName,
                                   streetDirection);
        await _houseService.Add(house);
        AddToClient(house);

        ColShapes.Add(house.Id,
                      Alt.CreateColShapeSphere(new Position(house.PositionX, house.PositionY, house.PositionZ), 1.5f));
    }

    public async Task CreateLeaseCompany(LeaseCompanyType leaseCompanyType, Position position, Rotation rotation,
                                         int price, string subName)
    {
        if (subName == "FREI")
        {
            subName = "";
        }

        var leaseCompanyHouse = new LeaseCompanyHouseModel(leaseCompanyType,
                                                           position.X,
                                                           position.Y,
                                                           position.Z,
                                                           rotation.Roll,
                                                           rotation.Pitch,
                                                           rotation.Yaw,
                                                           price,
                                                           subName);

        await _houseService.Add(leaseCompanyHouse);
        AddToClient(leaseCompanyHouse);

        ColShapes.Add(leaseCompanyHouse.Id,
                      Alt.CreateColShapeSphere(new Position(leaseCompanyHouse.PositionX,
                                                            leaseCompanyHouse.PositionY,
                                                            leaseCompanyHouse.PositionZ),
                                               1.5f));
    }

    public async Task DestroyHouse(HouseModel houseModel)
    {
        if (ColShapes.ContainsKey(houseModel.Id))
        {
            ColShapes[houseModel.Id].Remove();
        }

        AltAsync.EmitAllClients("houses:remove", houseModel.Id);
        await UpdateOwnerHouses(houseModel);

        if (houseModel.Inventory != null)
        {
            await _itemService.RemoveRange(houseModel.Inventory.Items);
            await _inventoryService.Remove(houseModel.Inventory);
        }

        await _houseService.Remove(houseModel);
    }

    public async Task Enter(ServerPlayer player)
    {
        var house = await _houseService.GetByDistance(player.Position);
        if (house == null || house.InteriorId == -1)
        {
            player.SendNotification("Es ist kein Haus in der Nähe deines Charakters.", NotificationType.ERROR);
            return;
        }

        if (!house.InteriorId.HasValue)
        {
            return;
        }

        if (player.IsInventoryOpen)
        {
            player.SendNotification(
                "Du kannst das Haus nicht betreten während dein Charakter sein Inventar offen hast.",
                NotificationType.ERROR);
            return;
        }

        if (house.LockState == LockState.CLOSED)
        {
            player.SendNotification("Dein Charakter kann das Haus nicht betreten, es ist abgeschlossen.",
                                    NotificationType.ERROR);
            return;
        }

        if (house.LockState == LockState.BROKEN)
        {
            player.SendNotification("Dein Charakter bemerkt wie das Schloss kaputt ist.", NotificationType.WARNING);
        }

        var exitPos = new Position(_worldLocationOptions.IntPositions[house.InteriorId.Value].X,
                                   _worldLocationOptions.IntPositions[house.InteriorId.Value].Y,
                                   _worldLocationOptions.IntPositions[house.InteriorId.Value].Z);

        await AltAsync.Do(() =>
        {
            player.Dimension = (int)house.Id;
            player.Position = exitPos;
            player.EmitLocked("player:setinhouse", true);
        });
    }

    public async Task Exit(ServerPlayer player)
    {
        if (player.Dimension == 0)
        {
            player.SendNotification("Du bist mit deinem Charakter in keinem Interior.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(player.Dimension);
        if (house == null || house.InteriorId == -1)
        {
            player.SendNotification("Bitte kontaktiere einen Admin dein Haus scheint nicht mehr zu exsistieren.",
                                    NotificationType.ERROR);
            return;
        }

        if (!house.InteriorId.HasValue)
        {
            return;
        }

        if (player.IsInventoryOpen)
        {
            player.SendNotification(
                "Du kannst das Haus nicht verlassen während dein Charakter sein Inventar offen hast.",
                NotificationType.ERROR);
            return;
        }

        if (house.LockState == LockState.CLOSED)
        {
            player.SendNotification("Dein Charakter kann das Haus nicht verlassen, es ist abgeschlossen.",
                                    NotificationType.ERROR);
            return;
        }

        if (house.LockState == LockState.BROKEN)
        {
            player.SendNotification("Dein Charakter bemerkt wie das Schloss kaputt ist.", NotificationType.WARNING);
            return;
        }

        var exitPos = new Position(_worldLocationOptions.IntPositions[house.InteriorId.Value].X,
                                   _worldLocationOptions.IntPositions[house.InteriorId.Value].Y,
                                   _worldLocationOptions.IntPositions[house.InteriorId.Value].Z);

        if (exitPos.Distance(player.Position) >= 2)
        {
            player.SendNotification("Es ist kein Ausgang in deiner Nähe", NotificationType.WARNING);
            return;
        }

        await AltAsync.Do(() =>
        {
            player.Dimension = 0;
            player.Position = new Position(house.PositionX, house.PositionY, house.PositionZ);
            player.Rotation = new Rotation(house.Roll, house.Pitch, house.Yaw);
            player.EmitLocked("player:setinhouse", false);
        });
    }

    public async Task SetHouseLocation(HouseModel houseModel, Position position, Rotation rotation)
    {
        houseModel.PositionX = position.X;
        houseModel.PositionY = position.Y;
        houseModel.PositionZ = position.Z;

        houseModel.Roll = rotation.Roll;
        houseModel.Pitch = rotation.Pitch;
        houseModel.Yaw = rotation.Yaw;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);

        ColShapes[houseModel.Id].Position = position;
    }

    public async Task SetSubName(HouseModel houseModel, string name)
    {
        houseModel.SubName = name;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task SetHouseNumber(HouseModel houseModel, int houseNumber)
    {
        houseModel.HouseNumber = houseNumber;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task SetPrice(HouseModel houseModel, int price)
    {
        houseModel.Price = price;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task SetStreetDirection(HouseModel houseModel, int streetDirection)
    {
        houseModel.StreetDirection = streetDirection;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task SetType(LeaseCompanyHouseModel leaseCompany, LeaseCompanyType type)
    {
        leaseCompany.LeaseCompanyType = type;

        await _houseService.Update(leaseCompany);
        await UpdateOnClient(leaseCompany);
    }

    public async Task SetRentable(HouseModel houseModel, bool state)
    {
        houseModel.Rentable = state;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task SetBlockedOwnerShip(HouseModel houseModel, bool state)
    {
        houseModel.BlockedOwnership = state;

        await _houseService.Update(houseModel);
        await UpdateOnClient(houseModel);
    }

    public async Task SetCashier(LeaseCompanyHouseModel leaseCompany, Position position, float heading)
    {
        leaseCompany.CashierX = position.X;
        leaseCompany.CashierY = position.Y;
        leaseCompany.CashierZ = position.Z;
        leaseCompany.CashierHeading = heading;

        await _houseService.Update(leaseCompany);
        _pedSyncModule.UpdateCashier(leaseCompany, position, heading);
        await UpdateOnClient(leaseCompany);
    }

    public async Task ClearCashier(LeaseCompanyHouseModel leaseCompany)
    {
        leaseCompany.CashierX = null;
        leaseCompany.CashierY = null;
        leaseCompany.CashierZ = null;
        leaseCompany.CashierHeading = null;

        await _houseService.Update(leaseCompany);
        await UpdateOnClient(leaseCompany);
    }

    public async Task SelectHouseInCreation(ServerPlayer player, int houseId)
    {
        var house = await _houseService.GetByKey(houseId);

        if (_playerSelections.ContainsKey(houseId) || house.CharacterModelId.HasValue)
        {
            player.SendNotification("Diese Immobilie ist leider schon vergeben.", NotificationType.ERROR);
            return;
        }

        var allSelections = _playerSelections.ToList().FindAll(p => p.Value == player);
        foreach (var item in allSelections)
        {
            _playerSelections.Remove(item.Key);
        }

        _playerSelections.Add(houseId, player);

        player.SendNotification("Diese Immobilie wurde deinem Charakter erfolgreich vorgemerkt.",
                                NotificationType.SUCCESS);
        player.Emit("houseselector:select", houseId);
    }

    public void UnselectHouseInCreation(ServerPlayer player, bool withMessage)
    {
        var allSelections = _playerSelections.ToList().FindAll(p => p.Value == player);
        foreach (var item in allSelections)
        {
            _playerSelections.Remove(item.Key);
        }

        if (withMessage)
        {
            player.SendNotification("Du hast die Immobilie für den Markt freigegeben.", NotificationType.INFO);
        }
    }

    public async Task<HouseModel?> GetStarterHouse(ServerPlayer player)
    {
        int? houseId = null;

        if (_playerSelections.ContainsValue(player))
        {
            var playerHouseSelection = _playerSelections.First(p => p.Value == player);
            houseId = playerHouseSelection.Key;
        }

        if (houseId == null)
        {
            return null;
        }

        return await _houseService.GetByKey(houseId);
    }

    public async Task UpdateHouses()
    {
        var houses = await GetStarterHousesAsync();
        var houseSplit = houses.Split(100);

        foreach (var houseSet in houseSplit)
        {
            foreach (var player in _playerSelections.Values)
            {
                player.EmitLocked("houseselector:updatechunk", houses);
            }
        }
    }

    public bool IsHouseBlocked(int houseId)
    {
        var used = false;

        _playerSelections.TryGetValue(houseId, out var buyer);
        if (buyer != null)
        {
            used = true;
        }

        return used;
    }

    public async Task Sync(ServerPlayer player)
    {
        var houses = await _houseService.GetAll();
        houses.ForEach(h => h.SouthCentralPoints = _southCentralPointsModule.GetPointsPrice(h.Price));

        var houseSplit = houses.Split(100);

        foreach (var houseSet in houseSplit)
        {
            player.EmitLocked("houses:syncchunk", houseSet);
        }

        player.EmitLocked("houses:syncexits", _worldLocationOptions.IntPositions.ToList());
    }

    public async Task UpdateOnClient(HouseModel houseModel)
    {
        AltAsync.EmitAllClients("houses:update", houseModel);
        await UpdateOwnerHouses(houseModel);
    }

    public void AddToClient(HouseModel houseModel)
    {
        AltAsync.EmitAllClients("houses:add", houseModel);
    }

    public async Task UpdateUi(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var houses = await _houseService.Where(h => h.CharacterModelId == player.CharacterModel.Id);
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);

        foreach (var group in groups)
        {
            var groupHouses = await _houseService.Where(h => h.GroupModelId == group.Id);

            foreach (var groupHouse in groupHouses.Where(groupHouse => houses.Find(h => h.Id == groupHouse.Id) == null))
            {
                houses.Add(groupHouse);
            }
        }

        player.EmitLocked("house:updatecharacterhouses", houses);
    }

    private bool IsStarterHouse(HouseModel houseModel)
    {
        return houseModel.SouthCentralPoints <= _characterCreatorOptions.MaxSouthCentralPointsHouses &&
               houseModel.HasNoOwner;
    }

    private async Task<List<HouseModel>> GetStarterHousesAsync()
    {
        var houses = await _houseService.Where(h => h.HouseType == HouseType.HOUSE);
        houses.ForEach(h => h.SouthCentralPoints = _southCentralPointsModule.GetPointsPrice(h.Price));

        return houses.FindAll(IsStarterHouse);
    }

    private async Task UpdateOwnerHouses(HouseModel houseModel)
    {
        if (houseModel.CharacterModelId.HasValue)
        {
            var player = Alt.GetAllPlayers().FindPlayerByCharacterId(houseModel.CharacterModelId.Value);
            if (player != null)
            {
                await UpdateUi(player);
            }
        }
    }
}