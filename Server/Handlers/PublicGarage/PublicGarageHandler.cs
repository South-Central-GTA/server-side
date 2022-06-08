using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.Group;
using Server.Modules.PublicGarage;
using Server.Modules.Vehicles;

namespace Server.Handlers.PublicGarage;

public class PublicGarageHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly GameOptions _gameOptions;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly PublicGarageEntryService _publicGarageEntryService;
    private readonly RegistrationOfficeService _registrationOfficeService;

    private readonly PublicGarageModule _publicGarageModule;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleModule _vehicleModule;

    private readonly VehicleService _vehicleService;
    private readonly WorldLocationOptions _worldLocationOptions;

    public PublicGarageHandler(
        IOptions<GameOptions> gameOptions,
        IOptions<WorldLocationOptions> worldLocationOptions,
        VehicleService vehicleService,
        PublicGarageEntryService publicGarageEntryService,
        BankAccountService bankAccountService,
        VehicleCatalogService vehicleCatalogService,
        GroupService groupService,
        RegistrationOfficeService registrationOfficeService,
        PublicGarageModule publicGarageModule,
        BankModule bankModule,
        VehicleModule vehicleModule,
        GroupModule groupModule)
    {
        _gameOptions = gameOptions.Value;
        _worldLocationOptions = worldLocationOptions.Value;

        _vehicleService = vehicleService;
        _publicGarageEntryService = publicGarageEntryService;
        _bankAccountService = bankAccountService;
        _vehicleCatalogService = vehicleCatalogService;
        _groupService = groupService;

        _publicGarageModule = publicGarageModule;
        _bankModule = bankModule;
        _vehicleModule = vehicleModule;
        _groupModule = groupModule;
        _registrationOfficeService = registrationOfficeService;

        AltAsync.OnClient<ServerPlayer>("publicgarage:requestparkvehicle", OnRequestParkVehicle);
        AltAsync.OnClient<ServerPlayer>("publicgarage:requestparkedvehicles", OnRequestUnparkVehicles);
        AltAsync.OnClient<ServerPlayer>("publicgarage:requestdestroyedvehicles", OnRequestDestroyedVehicles);
        AltAsync.OnClient<ServerPlayer, int>("publicgarage:unparkvehicle", OnUnparkVehicle);
        AltAsync.OnClient<ServerPlayer, int, int>("publicgarage:parkvehicle", OnParkVehicle);
        AltAsync.OnClient<ServerPlayer, int, int>("publicgarage:respawnvehicle", OnRespawnVehicle);
        AltAsync.OnClient<ServerPlayer, int>("publicgarage:requestvehiclerespawnprice", OnRequestVehicleRespawn);

        AltAsync.OnColShape += OnColShape;
    }

    private async void OnRequestParkVehicle(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var publicGarageData =
            _worldLocationOptions.PublicGarages.Find(
                g => player.Position.Distance(new Position(g.PedPointX, g.PedPointY, g.PedPointZ)) <= 3);
        if (publicGarageData == null)
        {
            player.SendNotification("Es befindet keine Garage in der Nähe deines Charakters.", NotificationType.ERROR);
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                                    NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Deinem Charakter fehlt ein Bankkonto um sein Fahrzeug einzuparken.",
                                    NotificationType.ERROR);
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().GetClosest(new Position(publicGarageData.ParkingPointX,
                                                                   publicGarageData.ParkingPointY,
                                                                   publicGarageData.ParkingPointZ));
        if (vehicle == null || !vehicle.Exists)
        {
            player.SendNotification("Es befindet sich kein Fahrzeug auf dem Marker.", NotificationType.ERROR);
            return;
        }

        if (vehicle?.DbEntity == null)
        {
            player.SendNotification("Dieses Fahrzeug kannst du nicht einparken.", NotificationType.ERROR);
            return;
        }

        if (vehicle.DbEntity.CharacterModelId != player.CharacterModel.Id)
        {
            var gender = player.CharacterModel.Gender == GenderType.MALE ? "der Eigentümer" : "die Eigentümerin";
            player.SendNotification(
                $"Dein Charakter muss {gender} des Fahrzeuges sein, die Garage verweigert die Annahme.",
                NotificationType.ERROR);
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.DbEntity.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        var data = new object[1];
        data[0] = publicGarageData.Id;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = publicGarageData.Name,
            Description =
                $"Das Fahrzeug hier einzuparken würde dich bei jedem Zahltag <b>${(int)(catalogVehicle.Price * publicGarageData.CostsPercentageOfVehiclePrice)}</b> kosten.<br><br>Von welchem Bankkonto soll die Garage die Kosten abbuchen?",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Einparken",
            PrimaryButtonServerEvent = "publicgarage:parkvehicle"
        });
    }

    private async void OnRequestUnparkVehicles(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var publicGarageData =
            _worldLocationOptions.PublicGarages.Find(
                g => player.Position.Distance(new Position(g.PedPointX, g.PedPointY, g.PedPointZ)) <= 3);
        if (publicGarageData == null)
        {
            player.SendNotification("Es befindet keine Garage in der Nähe deines Charakters.", NotificationType.ERROR);
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                                    NotificationType.ERROR);
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var publicGarageEntries = await _publicGarageEntryService.Where(pge =>
                                                                            pge.CharacterModelId ==
                                                                            player.CharacterModel.Id &&
                                                                            pge.GarageId == publicGarageData.Id);

        if (publicGarageEntries.Count == 0)
        {
            player.SendNotification("Dein Charakter hat keine Fahrzeuge in dieser Garage eingeparkt.",
                                    NotificationType.ERROR);
            return;
        }

        var publicGarageEntryDatas = new List<PublicGarageEntryData>();

        foreach (var publicGarageEntry in publicGarageEntries)
        {
            if (publicGarageEntry.PlayerVehicleModel == null)
            {
                continue;
            }

            var catalogVehicle =
                await _vehicleCatalogService.GetByKey(publicGarageEntry.PlayerVehicleModel.Model.ToLower());
            if (catalogVehicle == null)
            {
                continue;
            }

            publicGarageEntryDatas.Add(new PublicGarageEntryData
            {
                Id = publicGarageEntry.PlayerVehicleModelId,
                DisplayName = catalogVehicle.DisplayName,
                DisplayClass = catalogVehicle.DisplayClass,
                Costs = (int)(catalogVehicle.Price *
                              publicGarageData.CostsPercentageOfVehiclePrice)
            });
        }

        player.EmitLocked("publicgarage:setupunpark", publicGarageEntryDatas);
    }

    private async void OnRequestDestroyedVehicles(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var publicGarageData =
            _worldLocationOptions.PublicGarages.Find(
                g => player.Position.Distance(new Position(g.PedPointX, g.PedPointY, g.PedPointZ)) <= 3);
        if (publicGarageData == null)
        {
            player.SendNotification("Es befindet sich keine Garage in der Nähe deines Charakters.",
                                    NotificationType.ERROR);
            return;
        }

        var destroyedVehicles = await _vehicleService.Where(v => v.VehicleState == VehicleState.DESTROYED);

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        if (groups != null)
        {
            destroyedVehicles = destroyedVehicles.FindAll(v =>
                                                              v.CharacterModelId == player.CharacterModel.Id ||
                                                              groups.Any(g => _groupModule.IsOwner(player, g) &&
                                                                              g.Id == v.GroupModelOwnerId));
        }
        else
        {
            destroyedVehicles = destroyedVehicles.FindAll(v => v.CharacterModelId == player.CharacterModel.Id);
        }

        if (destroyedVehicles.Count == 0)
        {
            player.SendNotification("Dein Charakter hat keine zerstörten Fahrzeuge.",
                                    NotificationType.ERROR);
            return;
        }

        var vehicleDatas = new List<VehicleData>();

        foreach (var vehicle in destroyedVehicles)
        {
            var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
            if (catalogVehicle == null)
            {
                continue;
            }

            vehicleDatas.Add(new VehicleData
            {
                Id = vehicle.Id,
                DisplayName = catalogVehicle.DisplayName,
                DisplayClass = catalogVehicle.DisplayClass
            });
        }

        player.EmitLocked("publicgarage:showrespawnvehiclelist", vehicleDatas);
    }

    private async void OnUnparkVehicle(ServerPlayer player, int publicGarageEntryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var vehicle = await _vehicleService.GetByKey(publicGarageEntryId);
        if (vehicle == null)
        {
            player.SendNotification("Es wurde kein Fahrzeug ausgewählt.", NotificationType.ERROR);
            return;
        }

        var publicGarageEntry = await _publicGarageEntryService.Find(pge => pge.PlayerVehicleModelId == vehicle.Id);
        if (publicGarageEntry == null)
        {
            return;
        }

        var publicGarageData = _publicGarageModule.FindGarage(publicGarageEntry.GarageId);
        if (publicGarageData == null)
        {
            player.SendNotification(
                "Die Garage in welches du dein Auto eingeparkt hast existiert nicht mehr, kontaktiere einen Adminstrator.",
                NotificationType.ERROR);
            return;
        }

        if (Alt.GetAllVehicles()
               .FirstOrDefault(v => v.Position.Distance(new Position(publicGarageData.ParkingPointX,
                                                                     publicGarageData.ParkingPointY,
                                                                     publicGarageData.ParkingPointZ)) <= 2) != null
            || Alt.GetAllPlayers()
                  .FirstOrDefault(v => v.Position.Distance(new Position(publicGarageData.ParkingPointX,
                                                                        publicGarageData.ParkingPointY,
                                                                        publicGarageData.ParkingPointZ)) <= 2) != null)
        {
            player.SendNotification("Die Garage konnte dein Fahrzeug nicht ausparken, da der Stellplatz besetzt ist.",
                                    NotificationType.ERROR);
            return;
        }

        await _publicGarageModule.Unpark(player, vehicle, publicGarageData, publicGarageEntry);
    }

    private async void OnParkVehicle(ServerPlayer player, int bankAccountId, int garageId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Deinem Charakter fehlt ein Bankkonto um sein Fahrzeug einzuparken.",
                                    NotificationType.ERROR);
            return;
        }

        var publicGarageData =
            _worldLocationOptions.PublicGarages.Find(
                g => player.Position.Distance(new Position(g.PedPointX, g.PedPointY, g.PedPointZ)) <= 3);
        if (publicGarageData == null)
        {
            player.SendNotification("Es befindet keine Garage in der Nähe deines Charakters.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().GetClosest(new Position(publicGarageData.ParkingPointX,
                                                                   publicGarageData.ParkingPointY,
                                                                   publicGarageData.ParkingPointZ));
        if (vehicle == null || !vehicle.Exists)
        {
            player.SendNotification("Es befindet sich kein Fahrzeug auf dem Marker.", NotificationType.ERROR);
            return;
        }

        if (vehicle?.DbEntity == null)
        {
            player.SendNotification("Dieses Fahrzeug kannst du nicht einparken.", NotificationType.ERROR);
            return;
        }

        if (vehicle.DbEntity.CharacterModelId != player.CharacterModel.Id)
        {
            var gender = player.CharacterModel.Gender == GenderType.MALE ? "der Eigentümer" : "die Eigentümerin";
            player.SendNotification(
                $"Dein Charakter muss {gender} des Fahrzeuges sein, die Garage verweigert die Annahme.",
                NotificationType.ERROR);
            return;
        }

        if (vehicle.DbEntity.VehicleState != VehicleState.SPAWNED)
        {
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
            player.SendNotification("Dein Charakter hat keine Überweisungsrechte auf dem Bankkonto.",
                                    NotificationType.ERROR);
            return;
        }

        await _publicGarageModule.Park(player, vehicle, garageId, bankAccount.Id);
    }

    private async void OnRespawnVehicle(ServerPlayer player, int bankAccountId, int vehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Deinem Charakter fehlt ein Bankkonto um sein Fahrzeug zu spawnen.",
                                    NotificationType.ERROR);
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = await _vehicleService.GetByKey(vehicleId);
        if (vehicle == null)
        {
            return;
        }

        var groups = await _groupService.GetByOwner(player.CharacterModel.Id);
        if (groups != null)
        {
            if (vehicle.CharacterModelId != player.CharacterModel.Id
                && groups.All(g => g.Id != vehicle.GroupModelOwnerId))
            {
                return;
            }
        }
        else
        {
            if (vehicle.CharacterModelId != player.CharacterModel.Id)
            {
                return;
            }
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            player.SendNotification("Dein Charakter hat keine Überweisungsrechte auf dem Bankkonto.",
                                    NotificationType.ERROR);
            return;
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        var costs = (int)(catalogVehicle.Price * _gameOptions.RepairVehiclePercentage);
        await _bankModule.Withdraw(bankAccount, costs, true, "Fahrzeugreparatur");

        var publicGarageData = _worldLocationOptions.PublicGarages.Find(p =>
                                                                            new Position(p.ParkingPointX,
                                                                                         p.ParkingPointY,
                                                                                         p.ParkingPointZ)
                                                                                .Distance(player.Position) <= 20);
        if (publicGarageData == null)
        {
            player.SendNotification("Es befindet sich keine Garage in der Nähe deines Charakters.",
                                    NotificationType.ERROR);
            return;
        }

        if (Alt.GetAllVehicles().FirstOrDefault(v => v.Position.Distance(new Position(publicGarageData.ParkingPointX,
                                                                                      publicGarageData.ParkingPointY,
                                                                                      publicGarageData.ParkingPointZ)) <
                                                     2) != null)
        {
            player.SendNotification("Die Garage konnte dein Fahrzeug nicht ausparken, da der Stellplatz besetzt ist.",
                                    NotificationType.ERROR);
            return;
        }

        if (Alt.GetAllPlayers().FirstOrDefault(v => v.Position.Distance(new Position(publicGarageData.ParkingPointX,
                                                                                     publicGarageData.ParkingPointY,
                                                                                     publicGarageData.ParkingPointZ)) <
                                                    2) != null)
        {
            player.SendNotification("Die Garage konnte dein Fahrzeug nicht ausparken, da der Stellplatz besetzt ist.",
                                    NotificationType.ERROR);
            return;
        }

        var destroyedVehicle = Alt.GetAllVehicles().FindByDbId(vehicleId);
        if (destroyedVehicle is { Exists: true })
        {
            await destroyedVehicle.RemoveAsync();
        }

        vehicle.BodyHealth = 1000;
        vehicle.EngineHealth = 1000;

        vehicle.VehicleState = VehicleState.SPAWNED;
        vehicle.LockState = LockState.CLOSED;

        vehicle.PositionX = publicGarageData.ParkingPointX;
        vehicle.PositionY = publicGarageData.ParkingPointY;
        vehicle.PositionZ = publicGarageData.ParkingPointZ;
        await _vehicleService.Update(vehicle);

        await _vehicleModule.Create(vehicle);
    }

    private async void OnRequestVehicleRespawn(ServerPlayer player, int vehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        var vehicle = await _vehicleService.GetByKey(vehicleId);
        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        if (groups != null)
        {
            if (vehicle.CharacterModelId != player.CharacterModel.Id
                && groups.All(g => g.Id != vehicle.GroupModelOwnerId))
            {
                return;
            }
        }
        else
        {
            if (vehicle.CharacterModelId != player.CharacterModel.Id)
            {
                return;
            }
        }

        var catalogVehicle = await _vehicleCatalogService.GetByKey(vehicle.Model.ToLower());
        if (catalogVehicle == null)
        {
            return;
        }

        var data = new object[1];
        data[0] = vehicleId;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Fahrzeug spawnen",
            Description =
                $"Das Fahrzeug zu spawnen kostet <b>${(int)(catalogVehicle.Price * _gameOptions.RepairVehiclePercentage)}</b><br><br>Von welchem Bankkonto sollen es abgebucht werden?",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Respawnen",
            PrimaryButtonServerEvent = "publicgarage:respawnvehicle"
        });
    }

    private async Task OnColShape(IColShape colShape, IEntity targetEntity, bool state)
    {
        if (targetEntity is ServerPlayer player)
        {
            if (!player.Exists)
            {
                return;
            }

            if (!_publicGarageModule.ColShapes.ContainsValue(colShape))
            {
                return;
            }

            if (state)
            {
                player.SendSubtitle("Du kannst mit /park dein Fahrzeug parken.", 3000);
            }
            else
            {
                player.ClearSubtitle();
            }
        }
    }
}