using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Enums;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Database.Models.CustomLogs;
using Server.Database.Models.Group;
using Server.Database.Models.Housing;
using Server.Database.Models.Inventory;
using Server.Database.Models.Vehicles;
using Server.Modules.Admin;
using Server.Modules.Animation;
using Server.Modules.Bank;
using Server.Modules.Character;
using Server.Modules.Chat;
using Server.Modules.Clothing;
using Server.Modules.Death;
using Server.Modules.Discord;
using Server.Modules.Dump;
using Server.Modules.Group;
using Server.Modules.Houses;
using Server.Modules.Inventory;
using Server.Modules.Key;
using Server.Modules.Mail;
using Server.Modules.Vehicles;
using Server.Modules.World;

namespace Server.ChatCommands;

public class Administration : ISingletonScript
{
    private readonly AccountService _accountService;
    private readonly AdminPrisonModule _adminPrisonModule;
    private readonly AnimationModule _animationModule;
    private readonly AnimationService _animationService;
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly CharacterService _characterService;
    private readonly CharacterSpawnModule _characterSpawnModule;
    private readonly ChatModule _chatModule;
    private readonly CommandModule _commandModule;
    private readonly CompanyOptions _companyOptions;
    private readonly DefinedJobService _definedJobService;
    private readonly DiscordModule _discordModule;
    private readonly DoorModule _doorModule;

    private readonly FreecamModule _freecamModule;
    private readonly GameOptions _gameOptions;
    private readonly GroupFactionService _groupFactionService;
    private readonly GroupMemberService _groupMemberService;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly InventoryModule _inventoryModule;
    private readonly InventoryService _inventoryService;
    private readonly InventoryModule _inventorySpaceModule;
    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemService _itemService;
    private readonly LockModule _lockModule;
    private readonly MailModule _mailModule;
    private readonly ReviveModule _reviveModule;
    private readonly UserRecordLogService _userRecordLogService;
    private readonly VehicleCatalogService _vehicleCatalogService;
    private readonly VehicleDumpModule _vehicleDumpModule;
    private readonly VehicleModule _vehicleModule;
    private readonly VehicleService _vehicleService;
    private readonly WeatherModule _weatherModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public Administration(IOptions<WorldLocationOptions> worldLocationOptions, IOptions<CompanyOptions> companyOptions,
        IOptions<GameOptions> gameOptions, AccountService accountService, ItemCatalogService itemCatalogService,
        VehicleService vehicleService, HouseService houseService, VehicleCatalogService vehicleCatalogService,
        GroupService groupService, GroupFactionService groupFactionService, DefinedJobService definedJobService,
        CharacterService characterService, ItemService itemService, BankAccountService bankAccountService,
        UserRecordLogService userRecordLogService, AnimationService animationService, InventoryService inventoryService,
        FreecamModule freecamModule, InventoryModule inventorySpaceModule, DiscordModule discordModule,
        ItemCreationModule itemCreationModule, CharacterSpawnModule characterSpawnModule, LockModule lockModule,
        VehicleModule vehicleModule, VehicleDumpModule vehicleDumpModule, WeatherModule weatherModule,
        HouseModule houseModule, DoorModule doorModule, GroupMemberService groupMemberService, GroupModule groupModule,
        AdminPrisonModule adminPrisonModule, ChatModule chatModule, CommandModule commandModule,
        InventoryModule inventoryModule, BankModule bankModule, MailModule mailModule, AnimationModule animationModule,
        ReviveModule reviveModule)
    {
        _worldLocationOptions = worldLocationOptions.Value;
        _companyOptions = companyOptions.Value;
        _gameOptions = gameOptions.Value;

        _accountService = accountService;
        _itemCatalogService = itemCatalogService;
        _vehicleService = vehicleService;
        _houseService = houseService;
        _definedJobService = definedJobService;
        _groupService = groupService;
        _groupFactionService = groupFactionService;
        _groupMemberService = groupMemberService;
        _characterService = characterService;
        _itemService = itemService;
        _bankAccountService = bankAccountService;
        _userRecordLogService = userRecordLogService;
        _animationService = animationService;
        _inventoryService = inventoryService;

        _freecamModule = freecamModule;
        _inventorySpaceModule = inventorySpaceModule;
        _discordModule = discordModule;
        _itemCreationModule = itemCreationModule;
        _characterSpawnModule = characterSpawnModule;
        _lockModule = lockModule;
        _vehicleCatalogService = vehicleCatalogService;
        _vehicleModule = vehicleModule;
        _vehicleDumpModule = vehicleDumpModule;
        _weatherModule = weatherModule;
        _houseModule = houseModule;
        _doorModule = doorModule;
        _groupModule = groupModule;
        _adminPrisonModule = adminPrisonModule;
        _chatModule = chatModule;
        _commandModule = commandModule;
        _inventoryModule = inventoryModule;
        _bankModule = bankModule;
        _mailModule = mailModule;
        _animationModule = animationModule;
        _reviveModule = reviveModule;
    }

    [Command("aduty", "Gehe in den Admindienst sowie wieder in den Feierabend.", Permission.STAFF, null,
        CommandArgs.NOT_GREEDY, new[] { "adminduty" })]
    public async void OnAduty(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInFreeCam)
        {
            player.SendNotification("Du musst erst aus der Freecam raus.", NotificationType.ERROR);
            return;
        }

        player.IsAduty = !player.IsAduty;
        if (player.IsAduty)
        {
            player.SendNotification("Du bist in den Admindienst gegangen.", NotificationType.SUCCESS);
            await player.SetSyncedMetaDataAsync("NAMECOLOR", "~r~");
            await player.SetSyncedMetaDataAsync("CHARACTER_NAME", player.AccountName);
        }
        else
        {
            player.SendNotification("Du bist aus dem Admindienst gegangen.", NotificationType.SUCCESS);
            await player.SetSyncedMetaDataAsync("NAMECOLOR", "~w~");
            await player.SetSyncedMetaDataAsync("CHARACTER_NAME", player.CharacterModel.Name);
        }
    }

    [Command("addflag", "Setzt eine bestimmte Permission für den User.", Permission.TEAM_MANAGEMENT,
        new[] { "Spieler ID", "Permission" })]
    public async void OnAddFlag(ServerPlayer player, string expectedPlayerId, string expectedPermission)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe eine gültige Spieler ID an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!Enum.TryParse(expectedPermission, true, out Permission permission))
        {
            player.SendNotification("Es wurde keine Permission unter diesen Namen gefunden.", NotificationType.ERROR);
            return;
        }

        if (player.AccountModel.Permission != Permission.OWNER && permission == Permission.OWNER ||
            player.AccountModel.Permission != Permission.OWNER && permission == Permission.TEAM_MANAGEMENT)
        {
            player.SendNotification($"Du kannst nicht die Permission {permission} vergeben.", NotificationType.ERROR);
            return;
        }

        target.AccountModel.Permission |= permission;

        target.EmitLocked("chat:setcommands", _commandModule.GetAllCommand(player));
        target.EmitLocked("account:setpermissions", (int)player.AccountModel.Permission);

        target.SendNotification($"Du hast die Permission {permission} von {player.AccountName} hinzugefügt bekommen.",
            NotificationType.INFO);
        player.SendNotification($"Du hast {target.AccountName} die Permission {permission} hinzugefügt.",
            NotificationType.SUCCESS);

        await _accountService.Update(target.AccountModel);
    }

    [Command("removeflag", "Entfernt eine bestimmte Permission von dem User.", Permission.TEAM_MANAGEMENT,
        new[] { "Spieler ID", "Permission" })]
    public async void OnRemoveFlag(ServerPlayer player, string expectedPlayerId, string expectedPermission)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe eine gültige Spieler ID an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!Enum.TryParse(expectedPermission, true, out Permission permission))
        {
            player.SendNotification("Es wurde keine Permission unter diesen Namen gefunden.", NotificationType.ERROR);
            return;
        }

        if (player.AccountModel.Permission != Permission.OWNER && permission == Permission.OWNER ||
            player.AccountModel.Permission != Permission.OWNER && permission == Permission.TEAM_MANAGEMENT)
        {
            player.SendNotification($"Du kannst nicht die Permission {permission} entziehen.", NotificationType.ERROR);
            return;
        }

        target.AccountModel.Permission &= ~permission;

        target.EmitLocked("chat:setcommands", _commandModule.GetAllCommand(player));
        target.EmitLocked("account:setpermissions", (int)player.AccountModel.Permission);

        target.SendNotification($"Du hast die Permission {permission} von {player.AccountName} entzogen bekommen.",
            NotificationType.INFO);
        player.SendNotification($"Du hast {target.AccountName} die Permission {permission} entzogen.",
            NotificationType.SUCCESS);

        await _accountService.Update(target.AccountModel);
    }

    [Command("setflag", "Setzt die Permissions eines Users auf eine bestimmte.", Permission.TEAM_MANAGEMENT,
        new[] { "Spieler ID", "Permission" })]
    public async void OnSetFlag(ServerPlayer player, string expectedPlayerId, string expectedPermission)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe eine gültige Spieler ID an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!Enum.TryParse(expectedPermission, true, out Permission permission))
        {
            player.SendNotification("Es wurde keine Permission unter diesen Namen gefunden.", NotificationType.ERROR);
            return;
        }

        if (player.AccountModel.Permission != Permission.OWNER && permission == Permission.OWNER ||
            player.AccountModel.Permission != Permission.OWNER && permission == Permission.TEAM_MANAGEMENT)
        {
            player.SendNotification($"Du kannst nicht die Permission {permission} vergeben.", NotificationType.ERROR);
            return;
        }

        target.AccountModel.Permission = permission;

        target.EmitLocked("chat:setcommands", _commandModule.GetAllCommand(player));
        target.EmitLocked("account:setpermissions", (int)player.AccountModel.Permission);

        target.SendNotification($"Du hast die Permission {permission} von {player.AccountName} gesetzt bekommen.",
            NotificationType.INFO);
        player.SendNotification($"Du hast {target.AccountName} die Permission {permission} gesetzt.",
            NotificationType.SUCCESS);

        await _accountService.Update(target.AccountModel);
    }

    [Command("freecam", "Aktiviere und deaktivere die Freecam.", Permission.STAFF)]
    public async void OnFreecam(ServerPlayer player)
    {
        await AltAsync.Do(() =>
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

            lock (player)
            {
                player.IsInFreeCam = !player.IsInFreeCam;
            }

            if (player.IsInFreeCam)
            {
                _freecamModule.Start(player);
                player.SendNotification("Freecam aktiviert.", NotificationType.INFO);
            }
            else
            {
                _freecamModule.Stop(player);
                player.SendNotification("Freecam deaktiviert.", NotificationType.INFO);
            }
        });
    }

    [Command("bigears", "Schalte für dich den Umgebungschat ab und höre jeder Nachricht auf dem Server zu.",
        Permission.STAFF)]
    public async void OnBigEars(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            lock (player)
            {
                player.IsInBigEars = !player.IsInBigEars;
            }

            if (player.IsInBigEars)
            {
                player.SendNotification("BigEars aktiviert.", NotificationType.INFO);
            }
            else
            {
                player.SendNotification("BigEars deaktiviert.", NotificationType.INFO);
            }
        });
    }

    [Command("a", "Schreibe eine Nachricht im Admin Chat.", Permission.STAFF, new[] { "Text" }, CommandArgs.GREEDY)]
    public async void OnAdminChat(ServerPlayer player, string expectedText)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            expectedText = expectedText.TrimEnd().TrimStart();

            if (expectedText.Length == 0)
            {
                player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
                return;
            }

            foreach (var target in Alt.GetAllPlayers().Where(p => p.AccountModel.Permission.HasFlag(Permission.STAFF)))
            {
                _chatModule.SendMessage(target, player.AccountName, ChatType.ADMIN_CHAT, expectedText, "#e74c3c");
            }
        });
    }

    [Command("gethere", "Teleportiere den angegebenen Spieler zu deiner Position.", Permission.STAFF,
        new[] { "Spieler ID" })]
    public async void OnGetHere(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        await target.SetPositionAsync(player.Position);
        await target.SetDimensionAsync(player.Dimension);

        target.SendNotification($"Du wurdest von {player.AccountName} teleportiert.", NotificationType.INFO);
        player.SendNotification($"Du hast erfolgreich {target.AccountName} zu dir teleportiert.",
            NotificationType.INFO);
    }

    [Command("goto", "Teleportiere dich zu einem angegebenen Spieler.", Permission.STAFF, new[] { "Spieler ID" })]
    public async void OnGoTo(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (player == target)
        {
            player.SendNotification("Du kannst den Befehl nicht an dir selbst nutzen.", NotificationType.ERROR);
            return;
        }

        if (!player.IsInFreeCam)
        {
            await player.SetPositionAsync(target.Position);
        }
        else
        {
            _freecamModule.SetPosition(player, target.Position);
        }

        await player.SetDimensionAsync(target.Dimension);

        player.SendNotification($"Du hast dich zu {target.AccountName} teleportiert.", NotificationType.INFO);
    }

    [Command("freeze", "Freeze oder unfreeze den angegebenen Spieler.", Permission.STAFF, new[] { "Spieler ID" })]
    public async void OnFreeze(ServerPlayer player, string expectedPlayerId)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
            if (target == null)
            {
                return;
            }

            if (player == target)
            {
                player.SendNotification("Du kannst den Befehl nicht an dir selbst nutzen.", NotificationType.ERROR);
                return;
            }

            target.AdminFreezed = !target.AdminFreezed;
            var label = target.AdminFreezed ? "gefreezed" : "unfreezed";

            player.SendNotification($"Du hast {target.AccountName} {label}.", NotificationType.INFO);
        });
    }

    [Command("giveitem", "Gebe dem angegebenen Spieler ein Item aus dem Katalog.", Permission.TESTER,
        new[] { "Spieler ID", "Katalog Item Id", "Anzahl" })]
    public async void OnGiveItem(ServerPlayer player, string expectedPlayerId, string expectedCatalogItemId,
        string expectedAmount)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!Enum.TryParse(expectedCatalogItemId, out ItemCatalogIds catalogId))
        {
            player.SendNotification("Keine richtige Zahl als Item Id angegeben.", NotificationType.ERROR);
            return;
        }

        var catalogItem = await _itemCatalogService.GetByKey(catalogId);
        if (catalogItem == null)
        {
            player.SendNotification("Es wurde kein Item unter dieser ID gefunden.", NotificationType.ERROR);
            return;
        }

        if (ClothingModule.IsClothesOrProp(catalogItem.Id))
        {
            player.SendNotification("Kleidung kannst du so aus dem Katalog nicht erstellen.", NotificationType.ERROR);
            return;
        }

        if (catalogItem.Id == ItemCatalogIds.KEY)
        {
            player.SendNotification("Schlüssel kannst du so aus dem Katalog nicht erstellen.", NotificationType.ERROR);
            return;
        }

        if (catalogItem.Id == ItemCatalogIds.POLICE_TICKET)
        {
            player.SendNotification("Strafzettel kannst du so aus dem Katalog nicht erstellen.",
                NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedAmount, out var amount))
        {
            player.SendNotification("Bitte gebe ein richtige Anzahl an.", NotificationType.ERROR);
            return;
        }

        if (amount <= 0)
        {
            player.SendNotification("Bitte gebe ein positive Anzahl an.", NotificationType.ERROR);
            return;
        }

        if (!await _inventorySpaceModule.CanCarry(target, catalogItem.Id, amount))
        {
            return;
        }

        if (!catalogItem.Stackable && amount > 1)
        {
            amount = 1;
        }

        await _itemCreationModule.AddItemAsync(target, catalogItem.Id, amount);

        target.SendNotification(
            $"Dir wurde administrativ von {player.AccountName} x{amount} das Item '{catalogItem.Name}' gegeben.",
            NotificationType.INFO);
        player.SendNotification(
            $"Du hast erfolgreich {target.AccountName} x{amount} das Item '{catalogItem.Name}' gegeben.",
            NotificationType.INFO);
    }

    [Command("gotocoords", "Teleportiere dich zu einer bestimmte Koordinate.", Permission.STAFF,
        new[] { "X", "Y", "Z" })]
    public async void OnGoToCoords(ServerPlayer player, string expectedX, string expectedY, string expectedZ)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!float.TryParse(expectedX, out var x))
        {
            player.SendNotification("Gebe eine Zahl als X Wert an.", NotificationType.ERROR);
            return;
        }

        if (!float.TryParse(expectedY, out var y))
        {
            player.SendNotification("Gebe eine Zahl als Y Wert an.", NotificationType.ERROR);
            return;
        }

        if (!float.TryParse(expectedZ, out var z))
        {
            player.SendNotification("Gebe eine Zahl als Z Wert an.", NotificationType.ERROR);
            return;
        }

        await player.SetPositionAsync(new Position(x, y, z));
        player.SendNotification("Du hast dich erfolgreich teleportiert.", NotificationType.INFO);
    }

    [Command("savecam", "Speichere die aktuelle Position deiner Freecam Kamera ab.", Permission.STAFF,
        new[] { "Name" })]
    public void OnSaveCam(ServerPlayer player, string name)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInFreeCam)
        {
            player.SendNotification("Du musst in der Freecam sein um diesen Befehl zu nutzen.", NotificationType.ERROR);
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            player.SendNotification("Du musst einen Namen angeben unter welchem du die Position abspeichern möchtest.",
                NotificationType.ERROR);
            return;
        }

        player.EmitLocked("player:getcamerainfo", name);
    }

    [Command("savepos", "Speichere die aktuelle Position deines Charakters ab.", Permission.STAFF, new[] { "Name" })]
    public async void OnSavePosition(ServerPlayer player, string name)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInFreeCam)
        {
            player.SendNotification("Du musst aus der Freecam um diesen Befehl zu nutzen.", NotificationType.ERROR);
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            player.SendNotification("Du musst einen Namen angeben unter welchem du die Position abspeichern möchtest.",
                NotificationType.ERROR);
            return;
        }

        var pos = player.Position;
        DegreeRotation rot = player.Rotation;
        string savedPositionText;

        var dim = player.Dimension;

        if (player.IsInVehicle)
        {
            pos = player.Vehicle.Position;
            rot = player.Vehicle.Rotation;

            savedPositionText = string.Format(
                    "VehiclePosition: new LocationData(new Position({0}f, {1}f, {2}f), new Rotation({3}f, {4}f, {5}f)), Dimenson: {6}); // {7}\n",
                    pos.X.ToString().Replace(",", "."), pos.Y.ToString().Replace(",", "."),
                    (pos.Z - 1).ToString().Replace(",", "."), rot.Pitch.ToString().Replace(",", "."),
                    rot.Roll.ToString().Replace(",", "."), rot.Yaw.ToString().Replace(",", "."), dim, name)
                .ToString(new CultureInfo("en-US"));
        }
        else
        {
            savedPositionText = string.Format(
                    "PedPosition: new LocationData(new Position({0}f, {1}f, {2}f), new Rotation({3}f, {4}f, {5}f)), Dimenson: {6}); // {7}\n",
                    pos.X.ToString().Replace(",", "."), pos.Y.ToString().Replace(",", "."),
                    (pos.Z - 1f).ToString().Replace(",", "."), rot.Pitch.ToString().Replace(",", "."),
                    rot.Roll.ToString().Replace(",", "."), rot.Yaw.ToString().Replace(",", "."), dim, name)
                .ToString(new CultureInfo("en-US"));
        }

        await File.AppendAllTextAsync(@"savedpositions.txt", savedPositionText);

        player.SendNotification($"Position {name} wurde erfolgreich gespeichert", NotificationType.SUCCESS);
    }

    [Command("sethp", "Setze dem angegebenen Spieler die Lebenspunkte.", Permission.STAFF,
        new[] { "Spieler ID", "Lebenspunket" })]
    public async void OnSetHp(ServerPlayer player, string expectedPlayerId, string expectedHp)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!ushort.TryParse(expectedHp, out var hp))
        {
            player.SendNotification("Gebe eine Zahl als Lebenspunket an.", NotificationType.ERROR);
            return;
        }

        await target.SetHealthAsync(hp);
    }

    [Command("setarmor", "Setze dem angegebenen Spieler die Rüstungspunkte.", Permission.STAFF,
        new[] { "Spieler ID", "Rüstungspunkte" })]
    public async void OnSetArmor(ServerPlayer player, string expectedPlayerId, string expectedArmor)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!ushort.TryParse(expectedArmor, out var armor))
        {
            player.SendNotification("Gebe eine Zahl als Rüstungspunkte an.", NotificationType.ERROR);
            return;
        }

        await target.SetArmorAsync(armor);
    }

    [Command("spawn", "Spawne den angegebenen Spieler neu.", Permission.STAFF, new[] { "Spieler ID" })]
    public async void OnSpawn(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        await _characterSpawnModule.Spawn(target,
            new Position(_characterSpawnModule.GetSpawn(0).X, _characterSpawnModule.GetSpawn(0).Y,
                _characterSpawnModule.GetSpawn(0).Z),
            new Rotation(_characterSpawnModule.GetSpawn(0).Roll, _characterSpawnModule.GetSpawn(0).Pitch,
                _characterSpawnModule.GetSpawn(0).Yaw), 0);
    }

    [Command("alock", "Schließt ein Schloss administrativ auf oder zu.", Permission.STAFF)]
    public async void OnAdminLock(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var entity = await _lockModule.GetClosestLockableEntity(player);

        if (entity == null)
        {
            player.SendNotification("Es befindet sich kein Schloss in der Nähe.", NotificationType.ERROR);
            return;
        }

        await _lockModule.Lock(player, entity, true);
    }

    [Command("auncuff", "Nehmen einen Charakter administrativ Handschellen ab.", Permission.STAFF,
        new[] { "Spieler ID" })]
    public async void OnAUncuff(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        var itemHandCuff = (ItemHandCuffModel)target.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
            i.CatalogItemModelId == ItemCatalogIds.HANDCUFF && i.ItemState == ItemState.FORCE_EQUIPPED);
        if (itemHandCuff == null)
        {
            player.SendNotification("Der Charakter hat keine Handschellen angelegt.", NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.HANDCUFF))
        {
            return;
        }

        itemHandCuff.Slot = await _inventoryModule.GetFreeNextSlot(player.CharacterModel.InventoryModel.Id);
        ;
        itemHandCuff.InventoryModelId = player.CharacterModel.InventoryModel.Id;
        itemHandCuff.ItemState = ItemState.NOT_EQUIPPED;

        await _itemService.Update(itemHandCuff);

        target.Cuffed = false;
        target.UpdateClothes();

        await _inventoryModule.UpdateInventoryUiAsync(player);
        await _inventoryModule.UpdateInventoryUiAsync(target);

        target.SendNotification(
            $"Deinem Charakter wurden von {player.AccountName} administartiv die Handschellen abgenommen.",
            NotificationType.INFO);
        player.SendNotification(
            $"Du hast Charakter {target.CharacterModel.Name} administrativ Handschellen abgenommen.",
            NotificationType.SUCCESS);
    }

    [Command("aopeninventory", "Öffne administrativ das Inventar eines Charakters.", Permission.STAFF,
        new[] { "Charakter ID" })]
    public async void OnAOpenCharacterInventory(ServerPlayer player, string expectedCharacterId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedCharacterId, out var characterId))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl als Charakter Id an.", NotificationType.ERROR);
            return;
        }

        var inventory = await _inventoryService.Find(i => i.CharacterModelId == characterId);
        if (inventory == null)
        {
            player.SendNotification("Es wurde kein Inventar zum Charakter mit der Id gefunden.",
                NotificationType.ERROR);
            return;
        }

        player.OpenInventories = new List<OpenInventoryData> { new(InventoryType.FRISK, inventory.Id) };

        await _inventoryModule.OpenInventoryUiAsync(player);

        player.SendNotification("Du hast administrativ das Inventar geöffnet.", NotificationType.INFO);
    }

    [Command("aopenginventory", "Öffne administrativ das Gruppen Inventar eines Charakters.", Permission.MOD,
        new[] { "Charakter ID", "Gruppen ID" })]
    public async void OnAOpenGroupCharacterInventory(ServerPlayer player, string expectedCharacterId,
        string expectedGroupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedCharacterId, out var characterId))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl als Charakter Id an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl als Gruppen Id an.", NotificationType.ERROR);
            return;
        }

        var inventory = await _inventoryService.Find(i => i.GroupCharacterId == characterId && i.GroupId == groupId);
        if (inventory == null)
        {
            player.SendNotification("Es wurde kein Gruppen Inventar zum Charakter mit der Id gefunden.",
                NotificationType.ERROR);
            return;
        }

        player.OpenInventories = new List<OpenInventoryData> { new(inventory.InventoryType, inventory.Id) };

        await _inventoryModule.OpenInventoryUiAsync(player);

        player.SendNotification("Du hast administrativ das Gruppen Inventar geöffnet.", NotificationType.INFO);
    }

    [Command("setweather", "Ändere das aktuelle Wetter.", Permission.LEAD_AMIN,
        new[] { "Wetter ID", "Übergangszeit in Sekunden" })]
    public async void OnSetWeather(ServerPlayer player, string expectedWeather, string expectedTransitionTime)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            if (!Enum.TryParse(expectedWeather, true, out WeatherType weather))
            {
                player.SendNotification("Es wurde kein Wetter unter diesen Namen gefunden.", NotificationType.ERROR);
                return;
            }

            if (!int.TryParse(expectedTransitionTime, out var transitionTime))
            {
                player.SendNotification("Keine richtige Zahl als Übergangszeit angegeben.", NotificationType.ERROR);
                return;
            }

            _weatherModule.SetWeather(weather, transitionTime);
            player.SendNotification("Du hast das Wetter angepasst.", NotificationType.SUCCESS);
        });
    }

    [Command("arevive", "Belebe einen Charakter administrativ wieder.", Permission.STAFF, new[] { "Spieler ID" })]
    public async void OnReviveCharacter(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (target.CharacterModel.DeathState == DeathState.ALIVE)
        {
            player.SendNotification("Der angegebene Charakter ist nicht am Boden.", NotificationType.ERROR);
            return;
        }

        await _reviveModule.AutoRevivePlayer(target, target.Position);

        player.SendNotification("Du hast den Charakter " + target.CharacterModel.Name + " administrativ wiederbelebt.",
            NotificationType.SUCCESS);
        target.SendNotification("Du wurdest von " + player.AccountName + " administrativ wiederbelebt.",
            NotificationType.SUCCESS);
    }

    #region Lore and Event Management

    [Command("globalemote", "Definiere eine sichtbare Aktion für den ganzen Server.",
        Permission.HEAD_LORE_AND_EVENT_MANAGEMENT, new[] { "Tätigkeit" }, CommandArgs.GREEDY, new[] { "gme" })]
    public async void OnGlobalEmote(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        foreach (var target in Alt.GetAllPlayers().GetAllServerPlayers())
        {
            _chatModule.SendMessage(target, null, ChatType.EMOTE, message, "#C2A2DA");
        }
    }

    [Command("globalooc", "Definiere eine sichtbare Aktion für den ganzen Server.", Permission.ADMIN,
        new[] { "Nachricht" }, CommandArgs.GREEDY, new[] { "gooc" })]
    public async void OnGlobalOoc(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        foreach (var target in Alt.GetAllPlayers().GetAllServerPlayers())
        {
            _chatModule.SendMessage(target, player.AccountName, ChatType.OOC, message, "#E6E6E6");
        }
    }

    #endregion

    #region Vehicle Management

    [Command("addvehiclekey", "Füge dem angegebenen Spieler einen Fahrzeugschlüssel im Inventar dazu.",
        Permission.ADMIN, new[] { "Spieler ID", "Fahrzeug ID" })]
    public async void OnAddVehicle(ServerPlayer player, string expectedPlayerId, string expectedVehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!int.TryParse(expectedVehicleId, out var vehicleId))
        {
            player.SendNotification("Keine richtige Zahl als Fahrzeug ID angegeben.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleId);
        if (vehicle is not { Exists: true })
        {
            player.SendNotification("Es wurde kein persistentes Fahrzeug gefunden.", NotificationType.ERROR);
            return;
        }

        if (!await _inventorySpaceModule.CanCarry(target, ItemCatalogIds.KEY))
        {
            return;
        }

        await _vehicleModule.CreateVehicleKey(target, vehicle);

        player.SendNotification(
            $"Du hast {target.AccountName}, mit seinem Charakter {target.CharacterModel.Name} administrativ einen Schlüssel für das Fahrzeug Model {(VehicleModel)vehicle.Model} gegeben.",
            NotificationType.SUCCESS);
        target.SendNotification(
            $"Du hast von {player.AccountName}, administrativ einen Schlüssel für das Fahrzeug Model {(VehicleModel)vehicle.Model} erhalten.",
            NotificationType.INFO);
    }

    [Command("veh", "Spawnt ein beliebiges Fahrzeug.", Permission.TESTER,
        new[] { "Fahrzeug Model", "Primärfarbe", "Sekundärfarbe" })]
    public async void OnSpawnVehicle(ServerPlayer player, string expectedModelName, string expectedPrimaryColor,
        string expectedSecondaryColor)
    {
        if (!player.Exists)
        {
            return;
        }

        if (string.IsNullOrEmpty(expectedModelName))
        {
            player.SendNotification("Bitte gebe ein Fahrzeug an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrimaryColor, out var primaryColor))
        {
            player.SendNotification("Bitte gebe eine Primärfarbe an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedSecondaryColor, out var secondaryColor))
        {
            player.SendNotification("Bitte gebe eine Sekundärfarbe an.", NotificationType.ERROR);
            return;
        }

        if (primaryColor > 159 || secondaryColor > 159 || primaryColor < 0 ||
            secondaryColor < 0) // Max colors for vehicles
        {
            player.SendNotification("Deine Farben müssen zwischen 0 und 159 liegen.", NotificationType.ERROR);
            return;
        }

        var playerPosition = await player.GetPositionAsync();
        var playerRotation = await player.GetRotationAsync();

        var maxTank = 50;
        var catalogVehicle = await _vehicleCatalogService.GetByKey(expectedModelName);
        if (catalogVehicle != null)
        {
            maxTank = catalogVehicle.MaxTank;
        }

        var vehicle = await _vehicleModule.Create(expectedModelName, playerPosition, playerRotation, primaryColor,
            secondaryColor, 0, 1000, 1000, maxTank);
        if (vehicle != null)
        {
            player.SendNotification("Fahrzeug erfolgreich gespawnt.", NotificationType.SUCCESS);
            await player.SetIntoVehicleAsync(vehicle, 1);
        }
        else
        {
            player.SendNotification("Fahrzeug konnte nicht gefunden werden.", NotificationType.ERROR);
        }
    }

    [Command("aengine", "Starte den Motor des aktuellen persistenten Fahrzeuges administrativ.", Permission.STAFF)]
    public async void OnAdminEngine(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        await _vehicleModule.SetEngineState((ServerVehicle)player.Vehicle, player, !player.Vehicle.EngineOn, true);
    }

    [Command("afixveh", "Repariert ein Fahrzeug administrativ, schaltet ebenso den Motor dafür aus.", Permission.STAFF)]
    public async void OnAdminFix(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
                return;
            }

            var vehicle = (ServerVehicle)player.Vehicle;
            if (!vehicle.Exists)
            {
                return;
            }

            vehicle.SetRepairValue();

            player.SendNotification("Du hast das Fahrzeug repariert.", NotificationType.SUCCESS);
        });
    }

    [Command("arespawnveh", "Respawnt ein zerstörtes persistentes Fahrzeug an deiner Position.", Permission.STAFF,
        new[] { "Persistente Fahrzeug ID" })]
    public async void OnAdminVehicleRespawn(ServerPlayer player, string expectedVehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedVehicleId, out var vehicleId))
        {
            player.SendNotification("Keine richtige Zahl als Fahrzeug ID angegeben.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleId);
        if (vehicle is not { Exists: true })
        {
            player.SendNotification("Es wurde kein persistentes Fahrzeug gefunden.", NotificationType.ERROR);
            return;
        }

        if (!vehicle.IsDestroyed)
        {
            player.SendNotification("Das Fahrzeug ist nicht zerstört", NotificationType.ERROR);
            return;
        }

        await vehicle.RemoveAsync();

        vehicle.DbEntity.PositionX = player.Position.X;
        vehicle.DbEntity.PositionY = player.Position.Y;
        vehicle.DbEntity.PositionZ = player.Position.Z;

        vehicle.DbEntity.Roll = player.Rotation.Roll;
        vehicle.DbEntity.Pitch = player.Rotation.Pitch;
        vehicle.DbEntity.Yaw = player.Rotation.Yaw;

        vehicle.DbEntity.VehicleState = VehicleState.SPAWNED;

        await _vehicleModule.Create(vehicle.DbEntity);

        player.SendNotification($"Du hast das Fahrzeug Model: {vehicle.DbEntity.Model} respawnt.",
            NotificationType.SUCCESS);

        await _vehicleService.Update(vehicle.DbEntity);
    }

    [Command("getveh", "Teleportiere ein Fahrzeug zu dir.", Permission.STAFF, new[] { "Persistente Fahrzeug ID" })]
    public async void OnAdminGetVehicle(ServerPlayer player, string expectedVehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedVehicleId, out var vehicleId))
        {
            player.SendNotification("Keine richtige Zahl als Fahrzeug ID angegeben.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleId);
        if (vehicle is not { Exists: true })
        {
            player.SendNotification("Es wurde kein persistentes Fahrzeug gefunden.", NotificationType.ERROR);
            return;
        }

        await vehicle.SetPositionAsync(player.Position);
        await vehicle.SetRotationAsync(player.Rotation);
        await vehicle.SetDimensionAsync(player.Dimension);
        await player.SetPositionAsync(new Position(player.Position.X, player.Position.Y, player.Position.Z + 3));

        player.SendNotification($"Du hast das Fahrzeug Model: {vehicle.DbEntity.Model} zu dir teleportiert.",
            NotificationType.SUCCESS);
    }

    [Command("gotoveh", "Teleportiere dich zu einem Fahrzeug.", Permission.STAFF, new[] { "Persistente Fahrzeug ID" })]
    public async void OnAdminGotoVehicle(ServerPlayer player, string expectedVehicleId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedVehicleId, out var vehicleId))
        {
            player.SendNotification("Keine richtige Zahl als Fahrzeug ID angegeben.", NotificationType.ERROR);
            return;
        }

        var vehicle = Alt.GetAllVehicles().FindByDbId(vehicleId);
        if (vehicle is not { Exists: true })
        {
            player.SendNotification("Es wurde kein persistentes Fahrzeug gefunden.", NotificationType.ERROR);
            return;
        }

        await player.SetPositionAsync(vehicle.Position);
        await player.SetDimensionAsync(vehicle.Dimension);

        player.SendNotification($"Du hast das dich zum Fahrzeug Model: {vehicle.DbEntity.Model} teleportiert.",
            NotificationType.SUCCESS);
    }

    [Command("pveh", "Spawnt ein beliebiges Fahrzeug.", Permission.ADMIN,
        new[] { "Spieler ID", "Fahrzeug Model", "Primärfarbe", "Sekundärfarbe" })]
    public async void OnSpawnPlayerPersistentVehicle(ServerPlayer player, string targetPlayerName,
        string expectedModelName, string expectedPrimaryColor, string expectedSecondaryColor)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, targetPlayerName);
        if (target == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(expectedModelName))
        {
            player.SendNotification("Bitte gebe ein Fahrzeug an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrimaryColor, out var primaryColor))
        {
            player.SendNotification("Bitte gebe eine Primärfarbe an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedSecondaryColor, out var secondaryColor))
        {
            player.SendNotification("Bitte gebe eine Sekundärfarbe an.", NotificationType.ERROR);
            return;
        }

        if (primaryColor > 159 || secondaryColor > 159 || primaryColor < 0 ||
            secondaryColor < 0) // Max colors for vehicles
        {
            player.SendNotification("Deine Farben müssen zwischen 0 und 159 liegen.", NotificationType.ERROR);
            return;
        }

        var playerPosition = await player.GetPositionAsync();
        var playerRotation = await player.GetRotationAsync();
        var playerDimention = await player.GetDimensionAsync();

        var vehicle = await _vehicleModule.CreatePersistent(expectedModelName, target.CharacterModel, playerPosition, playerRotation,
            playerDimention, primaryColor, secondaryColor);
        if (vehicle == null)
        {
            player.SendNotification("Bitte gebe ein Fahrzeug an.", NotificationType.ERROR);
            return;
        }

        player.SendNotification($"Fahrzeug erfolgreich für {target.CharacterModel.Name} gespawnt.",
            NotificationType.SUCCESS);
    }

    [Command("gveh", "Spawnt ein beliebiges Fahrzeug.", Permission.ADMIN,
        new[] { "Gruppen ID", "Fahrzeug Model", "Primärfarbe", "Sekundärfarbe" })]
    public async void OnSpawnGroupPersistentVehicle(ServerPlayer player, string expectedGroupId,
        string expectedModelName, string expectedPrimaryColor, string expectedSecondaryColor)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine richtige Zahl als Gruppen ID an.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Die Gruppe existiert nicht.", NotificationType.ERROR);
            return;
        }

        if (string.IsNullOrEmpty(expectedModelName))
        {
            player.SendNotification("Bitte gebe ein Fahrzeug an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrimaryColor, out var primaryColor))
        {
            player.SendNotification("Bitte gebe eine Primärfarbe an.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedSecondaryColor, out var secondaryColor))
        {
            player.SendNotification("Bitte gebe eine Sekundärfarbe an.", NotificationType.ERROR);
            return;
        }

        if (primaryColor > 159 || secondaryColor > 159 || primaryColor < 0 ||
            secondaryColor < 0) // Max colors for vehicles
        {
            player.SendNotification("Deine Farben müssen zwischen 0 und 159 liegen.", NotificationType.ERROR);
            return;
        }

        var playerPosition = await player.GetPositionAsync();
        var playerRotation = await player.GetRotationAsync();
        var playerDimention = await player.GetDimensionAsync();

        await _vehicleModule.CreatePersistent(expectedModelName, group, playerPosition, playerRotation, playerDimention,
            primaryColor, secondaryColor);

        player.SendNotification($"Fahrzeug erfolgreich für Gruppe {group.Name} gespawnt.", NotificationType.SUCCESS);
    }

    [Command("dv", "Zerstört das Fahrzeug in welchem du sitzt.", Permission.TESTER)]
    public async void OnDestroyVehicle(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        var vehicle = player.Vehicle as ServerVehicle;
        if (vehicle == null)
        {
            return;
        }

        if (vehicle.DbEntity != null)
        {
            player.SendNotification("Du kannst persistente Fahrzeuge so nicht zerstören.", NotificationType.ERROR);
            return;
        }

        await vehicle.RemoveAsync();

        player.SendNotification("Du hast das Fahrzeug zerstört.", NotificationType.SUCCESS);
    }

    [Command("setlivery", "Setze die Lackierung von einem Fahrzeug.", Permission.TESTER, new[] { "Livery" })]
    public async void OnAdminSetVehicleLivery(ServerPlayer player, string expectedLivery)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (!byte.TryParse(expectedLivery, out var livery))
        {
            player.SendNotification("Keine richtige Zahl als Livery angegeben.", NotificationType.ERROR);
            return;
        }

        var vehicleDumpData = _vehicleDumpModule.Dump.Find(v => v.Hash == player.Vehicle.Model);
        if (vehicleDumpData != null)
        {
            if (!vehicleDumpData.Flags.Contains("FLAG_HAS_LIVERY"))
            {
                player.SendNotification(
                    "Dieses Fahrzeug hat keine extra Lackierungen. (unterschiedliche Sorten wie Aufkleber etc.)",
                    NotificationType.ERROR);
                return;
            }
        }

        await player.Vehicle.SetLiveryAsync(livery);

        var vehicle = (ServerVehicle)player.Vehicle;
        if (vehicle is { DbEntity: { } })
        {
            vehicle.DbEntity.Livery = livery;
            await _vehicleService.Update(vehicle.DbEntity);
        }

        player.SendNotification("Du hast die Livery des Fahrzeuges geändert.", NotificationType.SUCCESS);
    }

    [Command("setvehprice", "Setze den Preis für das Fahrzeug im Fahrzeugkatalog.", Permission.STAFF,
        new[] { "Preis in Dollar" })]
    public async void OnSetVehiclePrice(ServerPlayer player, string expectedPrice)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrice, out var price))
        {
            player.SendNotification("Keine richtige Zahl als Preis angegeben.", NotificationType.ERROR);
            return;
        }

        if (price < 0)
        {
            player.SendNotification("Keine positive Zahl als Preis angegeben.", NotificationType.ERROR);
            return;
        }

        var modelName = ((VehicleModel)player.Vehicle.Model).ToString().ToLower();
        var catalogVehicle = await _vehicleCatalogService.GetByKey(modelName);
        if (catalogVehicle == null)
        {
            player.EmitLocked("vehiclecatalog:getcatalogveh",
                new CatalogVehicleModel { Model = modelName, Price = price });
            return;
        }

        catalogVehicle.Price = price;

        await _vehicleCatalogService.Update(catalogVehicle);

        player.SendNotification("Du hast den Preis des Fahrzeuges geändert.", NotificationType.SUCCESS);
    }

    [Command("setvehfueltype", "Setze die Treibstoff Art für das Fahrzeug im Fahrzeugkatalog", Permission.STAFF,
        new[] { "Treibstoff Art (Muscle_Power, Diesel, Petrol, Kerosene, Electricity)" })]
    public async void OnSetVehFuelType(ServerPlayer player, string expectedFuelType)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedFuelType.ToUpper(), out FuelType fuelType))
        {
            player.SendNotification("Es wurde keine Treibstoff Art gefunden.", NotificationType.ERROR);
            return;
        }

        var modelName = ((VehicleModel)player.Vehicle.Model).ToString().ToLower();
        var catalogVehicle = await _vehicleCatalogService.GetByKey(modelName);
        if (catalogVehicle == null)
        {
            player.EmitLocked("vehiclecatalog:getcatalogveh",
                new CatalogVehicleModel { Model = modelName, FuelType = fuelType });
            return;
        }

        catalogVehicle.FuelType = fuelType;

        await _vehicleCatalogService.Update(catalogVehicle);

        player.SendNotification("Du hast die Treibstoff Art des Fahrzeuges geändert.", NotificationType.SUCCESS);
    }

    [Command("setvehmaxfuel", "Setze die maximale Treibstoffmenge für den Tank im Fahrzeugkatalog", Permission.STAFF,
        new[] { "Maximal Anzahl in Liter" })]
    public async void OnSetVehMaxFuel(ServerPlayer player, string expectedMaxFuel)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedMaxFuel, out var maxFuel))
        {
            player.SendNotification("Keine richtige Zahl als maximale Treibstoffmenge angegeben.",
                NotificationType.ERROR);
            return;
        }

        var modelName = ((VehicleModel)player.Vehicle.Model).ToString().ToLower();
        var catalogVehicle = await _vehicleCatalogService.GetByKey(modelName);
        if (catalogVehicle == null)
        {
            player.EmitLocked("vehiclecatalog:getcatalogveh",
                new CatalogVehicleModel { Model = modelName, MaxTank = maxFuel });
            return;
        }

        catalogVehicle.MaxTank = maxFuel;

        await _vehicleCatalogService.Update(catalogVehicle);

        player.SendNotification("Du hast die maximale Treibstoffmenge des Fahrzeuges geändert.",
            NotificationType.SUCCESS);
    }

    [Command("setvehfuel", "Setze die Treibstoffmenge für den Tank.", Permission.STAFF, new[] { "Anzahl in Liter" })]
    public async void OnSetVehFuel(ServerPlayer player, string expectedMaxFuel)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter muss in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedMaxFuel, out var fuel))
        {
            player.SendNotification("Keine richtige Zahl als Treibstoffmenge angegeben.", NotificationType.ERROR);
            return;
        }

        var modelName = ((VehicleModel)player.Vehicle.Model).ToString().ToLower();
        var catalogVehicle = await _vehicleCatalogService.GetByKey(modelName);
        if (catalogVehicle == null)
        {
            return;
        }

        if (fuel > catalogVehicle.MaxTank)
        {
            player.SendNotification($"Du kannst nicht mehr als {catalogVehicle.MaxTank} Liter in den Tank füllen.",
                NotificationType.ERROR);
            return;
        }

        if (player.Vehicle is not ServerVehicle vehicle)
        {
            return;
        }

        await _vehicleModule.SetVehicleFuel(player, vehicle, fuel);

        player.SendNotification("Du hast die Treibstoffmenge des Fahrzeuges geändert.", NotificationType.SUCCESS);
    }

    #endregion

    #region House Management

    [Command("addhousekey", "Füge dem angegebenen Spieler ein Hausschlüssel im Inventar dazu.",
        Permission.ECONOMY_MANAGEMENT, new[] { "Spieler ID", "Haus ID" })]
    public async void OnAddHouseKey(ServerPlayer player, string expectedPlayerId, string expectedHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        if (!await _inventorySpaceModule.CanCarry(target, ItemCatalogIds.KEY))
        {
            return;
        }

        await _houseModule.CreateHouseKey(player.CharacterModel, house);

        player.SendNotification(
            $"Du hast {target.AccountName}, mit seinem Charakter {target.CharacterModel.Name} administrativ einen Schlüssel für das Haus {houseId} gegeben.",
            NotificationType.SUCCESS);
        target.SendNotification(
            $"Du hast von {player.AccountName}, administrativ einen Schlüssel für Haus Id {houseId} erhalten.",
            NotificationType.INFO);

        await _inventoryModule.UpdateInventoryUiAsync(target);
    }

    [Command("createhouse", "Erstelle an deiner aktuellen Position ein neues Haus.", Permission.HEAD_ECONOMY_MANAGEMENT,
        new[] { "Interior ID", "Preis", "Hausnummer", "Straßenrichtung", "Name (Kein Name = 'FREI')" })]
    public async void OnCreateHouse(ServerPlayer player, string expectedInteriorId, string expectedPrice,
        string expectedHouseNumber, string expectedStreetDirection, string expectedName)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedInteriorId, out var interiorId))
        {
            player.SendNotification("Keine richtige Zahl als Interior Id angegeben.", NotificationType.ERROR);
            return;
        }

        if (interiorId > _worldLocationOptions.IntPositions.Length + 1 || interiorId < 0)
        {
            player.SendNotification("Die Interior Id existiert nicht.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedHouseNumber, out var houseNumber))
        {
            player.SendNotification("Keine richtige Zahl als Hausnummer angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrice, out var price))
        {
            player.SendNotification("Keine richtige Zahl als Preis angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedStreetDirection, out var streetDirection))
        {
            player.SendNotification("Keine richtige Zahl als Straßenrichtung angegeben.", NotificationType.ERROR);
            return;
        }

        if (houseNumber < 0)
        {
            player.SendNotification("Die Hausnummer darf nicht negativ sein.", NotificationType.ERROR);
            return;
        }

        if (price < 0)
        {
            player.SendNotification("Der Preis darf nicht negativ sein.", NotificationType.ERROR);
            return;
        }

        if (streetDirection < 1 || streetDirection > 2)
        {
            player.SendNotification("Die Straßenrichtung muss zwischen entweder 1 oder 2 sein.",
                NotificationType.ERROR);
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Du kannst in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (player.Dimension != 0)
        {
            player.SendNotification("Du kannst in keinem Interior oder Dimension sein.", NotificationType.ERROR);
            return;
        }

        await _houseModule.CreateHouse(new Position(player.Position.X, player.Position.Y, player.Position.Z - 1),
            player.Rotation, interiorId, houseNumber, price, expectedName, streetDirection);

        player.SendNotification("Du hast ein Haus erstellt.", NotificationType.SUCCESS);
    }

    [Command("deletehouse", "Löscht das Haus an deiner aktuellen Position.", Permission.HEAD_ECONOMY_MANAGEMENT,
        new[] { "Haus ID" })]
    public async void OnDestroyHouse(ServerPlayer player, string expectedHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        if (house.GroupModelId.HasValue)
        {
            var group = await _groupService.GetByKey(house.GroupModelId.Value);
            player.SendNotification(
                $"Diese Haus hat das Unternehmen #{house.GroupModelId} {group.Name} als Hauptsitz und kann daher nicht gelöscht werden.",
                NotificationType.ERROR);
            return;
        }

        await _houseModule.DestroyHouse(house);

        player.SendNotification("Du hast das Haus gelöscht.", NotificationType.SUCCESS);
    }

    [Command("sethouseowner", "Setze einen bestimmten Spieler als Eigentümer des Hauses.",
        Permission.ECONOMY_MANAGEMENT, new[] { "Spieler ID", "Haus ID" })]
    public async void OnSetHouseOwner(ServerPlayer player, string expectedPlayerId, string expectedHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.SetOwner(target.CharacterModel, house);

        player.SendNotification(
            $"Du hast {target.AccountName} mit dem Charakter {target.CharacterModel.Name} als Eigentümer gesetzt.",
            NotificationType.SUCCESS);
    }

    [Command("clearhouseowner", "Entferne den Eigentümer eines Hauses.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID" })]
    public async void OnClearHouseOwner(ServerPlayer player, string expectedHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        if (house.HasNoOwner)
        {
            player.SendNotification("Dieses Haus hat keinen Eigentümer.", NotificationType.ERROR);
            return;
        }

        await _houseModule.ResetOwner(house);

        player.SendNotification("Du hast ehemalige Eigentümer entfernt.", NotificationType.SUCCESS);
    }

    [Command("gethouse", "Verschiebt die angegebene Haus Id an deine aktuelle Position.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID" })]
    public async void OnGetHouse(ServerPlayer player, string expectedHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.SetHouseLocation(house,
            new Position(player.Position.X, player.Position.Y, player.Position.Z - 1), player.Rotation);

        player.SendNotification("Du hast das Haus verschoben.", NotificationType.SUCCESS);
    }

    [Command("gotohouse", "Teleportiere dich zu einem bestimmten Haus.", Permission.STAFF, new[] { "Haus ID" })]
    public async void OnGotoHouse(ServerPlayer player, string expectedHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        await player.SetPositionAsync(new Position(house.PositionX, house.PositionY, house.PositionZ));

        player.SendNotification("Du hast dich zum Haus teleportiert.", NotificationType.SUCCESS);
    }

    [Command("sethousename", "Ändere den Namen welcher beim Haus angezeigt wird.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID", "Name (Kein Name = 'FREI')" }, CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnSetHouseName(ServerPlayer player, string expectedHouseId, string expectedName)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        var name = expectedName.Replace('_', ' ');

        if (expectedName == "FREI")
        {
            player.SendNotification("Du hast den Namen entfernt.", NotificationType.SUCCESS);
            await _houseModule.SetSubName(house, "");
        }
        else
        {
            player.SendNotification($"Du hast den Namen auf {name} gesetzt.", NotificationType.SUCCESS);
            await _houseModule.SetSubName(house, name);
        }
    }

    [Command("sethousenumber", "Ändere die Hausnummer welche beim Haus angezeigt wird.", Permission.MOD,
        new[] { "Haus ID", "Hausnummer" })]
    public async void OnSetHouseNumber(ServerPlayer player, string expectedHouseId, string expectedNumber)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedNumber, out var houseNumber))
        {
            player.SendNotification("Keine richtige Zahl als Hausnummer angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.SetHouseNumber(house, houseNumber);

        player.SendNotification("Hausnummer wurde angepasst.", NotificationType.SUCCESS);
    }

    [Command("sethouseprice", "Ändere den Preis welcher das Haus kosten soll.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID", "Preis" })]
    public async void OnSetHousePrice(ServerPlayer player, string expectedHouseId, string expectedPrice)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrice, out var price))
        {
            player.SendNotification("Keine richtige Zahl als Preis angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.SetPrice(house, price);

        player.SendNotification("Preis wurde angepasst.", NotificationType.SUCCESS);
    }

    [Command("sethousedirection", "Ändere den Namen der Straße welche beim Haus angezeigt wird.", Permission.MOD,
        new[] { "Haus ID", "Kreuzungsrichtung" })]
    public async void OnSetHouseStreetDirection(ServerPlayer player, string expectedHouseId, string expectedDirection)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedDirection, out var streetDirection))
        {
            player.SendNotification("Keine richtige Zahl als Preis angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        if (streetDirection < 1 || streetDirection > 2)
        {
            player.SendNotification("Die Straßenrichtung muss zwischen entweder 1 oder 2 sein.",
                NotificationType.ERROR);
            return;
        }

        await _houseModule.SetStreetDirection(house, streetDirection);

        player.SendNotification("Kreuzungsrichtung wurde angepasst.", NotificationType.SUCCESS);
    }

    [Command("addhousedoor", "Füge einem Haus eine bestimmte Tür hinzu.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID", "Tür Mesh", "Tür X Position", "Tür Y Position", "Tür Z Position" })]
    public async void OnAddHouseDoor(ServerPlayer player, string expectedHouseId, string expectedDoorMesh,
        string expectedDoorX, string expectedDoorY, string expectedDoorZ)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!uint.TryParse(expectedDoorMesh, out var doorMesh))
        {
            player.SendNotification("Keine richtige Zahl als Preis angegeben.", NotificationType.ERROR);
            return;
        }

        if (!float.TryParse(expectedDoorX, out var doorX))
        {
            player.SendNotification("Keine richtige Zahl als Tür X Position angegeben.", NotificationType.ERROR);
            return;
        }

        if (!float.TryParse(expectedDoorY, out var doorY))
        {
            player.SendNotification("Keine richtige Zahl als Tür Y Position angegeben.", NotificationType.ERROR);
            return;
        }

        if (!float.TryParse(expectedDoorZ, out var doorZ))
        {
            player.SendNotification("Keine richtige Zahl als Tür Z Position angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        await _doorModule.AddDoor(house, doorMesh, new Position(doorX, doorY, doorZ));

        player.SendNotification("Tür wurde hinzugefügt.", NotificationType.SUCCESS);
    }

    [Command("removehousedoor", "Entferne eine bestimmte Tür von einem Haus.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID", "Tür ID" })]
    public async void OnRemoveHouseDoor(ServerPlayer player, string expectedHouseId, string expectedDoorId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedDoorId, out var doorId))
        {
            player.SendNotification("Keine richtige Zahl als Tür ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        var success = await _doorModule.RemoveDoor(house, doorId);
        if (!success)
        {
            player.SendNotification("Es konnte keine Tür gefunden werden.", NotificationType.ERROR);
            return;
        }

        player.SendNotification("Tür wurde hinzugefügt.", NotificationType.SUCCESS);
    }

    [Command("sethouseinterior", "Setze oder entferne das Interior eines Hauses.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID", "Interior ID (-1 für kein Interior)" })]
    public async void OnSetHouseInterior(ServerPlayer player, string expectedHouseId, string expectedInteriorId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedInteriorId, out var interiorId))
        {
            player.SendNotification("Keine richtige Zahl als InteriorId ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (interiorId < -1)
        {
            player.SendNotification("Du kannst nicht weniger als -1 nehmen.", NotificationType.ERROR);
            return;
        }

        var maxInts = _worldLocationOptions.IntPositions.Length - 1;

        if (maxInts < interiorId)
        {
            player.SendNotification("Die maximale Anzahl an Interiors ist " + maxInts + ".", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein Haus gefunden.", NotificationType.ERROR);
            return;
        }

        house.InteriorId = interiorId != -1 ? interiorId : null;

        await _houseService.Update(house);

        player.SendNotification("Interior wurde geupdated.", NotificationType.SUCCESS);
    }

    // TODO: Remove command when going live.
    [Command("resetallhouses", "Resete alle Häuser damit sie ordentlich gespeichert werden können.", Permission.OWNER)]
    public async void OnResetAllHouses(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var houses = await _houseService.GetAll();

        foreach (var house in houses)
        {
            house.Keys = new List<int>();
            house.LockState = LockState.CLOSED;
            house.CharacterModelId = null;
            house.GroupModelId = null;
        }

        await _houseService.UpdateRange(houses);

        player.SendNotification("Alle Häuser wurden erfolgreich vorbereitet.", NotificationType.SUCCESS);
    }

    #endregion

    #region Group Management

    [Command("creategroup", "Erstelle eine Gruppe.", Permission.ADMIN, new[] { "Type", "Name" },
        CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnCreateGroup(ServerPlayer player, string expectedType, string expectedName)
    {
        if (!player.Exists)
        {
            return;
        }

        if (int.TryParse(expectedType, out var _))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedType, true, out GroupType groupType))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (expectedName.Length == 0)
        {
            player.SendNotification("Invalider Name.", NotificationType.ERROR);
            return;
        }

        var createdGroup = await _groupModule.CreateGroup(groupType, expectedName);
        if (createdGroup != null)
        {
            await _bankModule.CreateBankAccount(createdGroup);
            await _mailModule.CreateMailAccount(createdGroup);

            player.SendNotification("Du hast erfolgreich eine Gruppe erstellt.", NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification("Der Name ist schon vergeben, Gruppe konnte nicht erstellt werden.",
                NotificationType.ERROR);
        }
    }

    [Command("setgroupowner", "Setze den Owner einer Gruppe.", Permission.ADMIN, new[] { "Spieler ID", "Gruppen ID" })]
    public async void OnSetGroupOwner(ServerPlayer player, string expectedPlayerId, string expectedGroupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        var member = group.Members.Find(m => m.CharacterModelId == target.CharacterModel.Id);
        if (member != null)
        {
            member.Owner = true;
            await _groupMemberService.Update(member);
        }
        else
        {
            var bankAccounts = await _bankAccountService.GetByOwner(target.CharacterModel.Id);
            if (bankAccounts.Count == 0)
            {
                player.SendNotification("Charakter hat kein Bankkonto.", NotificationType.ERROR);
                return;
            }

            await _groupMemberService.Add(new GroupMemberModel
            {
                GroupModelId = group.Id,
                CharacterModelId = target.CharacterModel.Id,
                Owner = true,
                RankLevel = 1,
                BankAccountId = bankAccounts[0].Id
            });
        }

        await _groupModule.UpdateUi(target);
        player.SendNotification(
            $"Du hast erfolgreich {target.CharacterModel.Name} als Owner der Gruppe {group.Name} gesetzt.",
            NotificationType.SUCCESS);
    }

    [Command("cleargroupowner", "Entferne den Owner einer Gruppe.", Permission.ADMIN, new[] { "Gruppen ID" })]
    public async void OnClearGroupOwner(ServerPlayer player, string expectedGroupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        foreach (var ownerMembers in group.Members.FindAll(m => m.Owner))
        {
            ownerMembers.Owner = false;
            await _groupMemberService.Update(ownerMembers);
        }

        await _groupService.Update(group);

        player.SendNotification($"Du hast erfolgreich den Owner Gruppe {group.Name} entfernt.",
            NotificationType.SUCCESS);
    }

    [Command("setgroupname", "Setze den Namen einer Gruppe.", Permission.ADMIN, new[] { "Gruppen ID", "Name" },
        CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnSetGroupName(ServerPlayer player, string expectedGroupId, string expectedName)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (expectedName.Length == 0)
        {
            player.SendNotification("Invalider Name.", NotificationType.ERROR);
            return;
        }

        var oldName = group.Name;

        group.Name = expectedName;

        await _groupService.Update(group);

        player.SendNotification($"Du hast erfolgreich den Namen der Gruppe {oldName} auf {group.Name} geändert.",
            NotificationType.SUCCESS);
    }

    [Command("setgrouptype", "Setze den Typen einer Gruppe.", Permission.ADMIN, new[] { "Gruppen ID", "Typen" })]
    public async void OnSetGroupType(ServerPlayer player, string expectedFactionId, string expectedType)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedFactionId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (int.TryParse(expectedType, out var _))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedType, true, out GroupType groupType))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        var oldType = group.GroupType;

        group.GroupType = groupType;

        await _groupService.Update(group);

        player.SendNotification(
            $"Du hast erfolgreich den Typen der Gruppe von {oldType} auf {group.GroupType} geändert.",
            NotificationType.SUCCESS);
    }

    [Command("setfactiontype", "Setze den Faction Typen einer Fraktion.", Permission.ADMIN,
        new[] { "Fraktion ID", "Typen" })]
    public async void OnSetFactionType(ServerPlayer player, string expectedFactionId, string expectedType)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedFactionId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Fraktions ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (int.TryParse(expectedType, out var _))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedType, true, out FactionType factionType))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        var faction = await _groupFactionService.GetByKey(groupId);
        if (faction == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        var oldType = faction.FactionType;

        faction.FactionType = factionType;

        await _groupFactionService.Update(faction);

        player.SendNotification(
            $"Du hast erfolgreich den Typen der Fraktion von {oldType} auf {faction.FactionType} geändert.",
            NotificationType.SUCCESS);
    }

    [Command("setgroupproducts", "Setze die Produkte einer Gruppe.", Permission.ADMIN,
        new[] { "Gruppen ID", "Anzahl" })]
    public async void OnSetGroupProducts(ServerPlayer player, string expectedGroupId, string expectedAmount)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedAmount, out var amount))
        {
            player.SendNotification("Keine richtige Zahl als Anzahl angegeben.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        var companyGroup = (CompanyGroupModel)group;

        if (amount > _companyOptions.MaxProducts)
        {
            player.SendNotification($"Du kannst nicht mehr als {_companyOptions.MaxProducts} Produkte setzen.",
                NotificationType.ERROR);
            return;
        }

        companyGroup.Products = amount;

        await _groupService.Update(group);

        player.SendNotification($"Du hast erfolgreich die Produkte der Gruppe auf {amount} gesetzt.",
            NotificationType.SUCCESS);
    }

    [Command("addplayergroup", "Füge einen Spieler als Member einer Gruppe hinzu.", Permission.ADMIN,
        new[] { "Spieler ID", "Gruppen ID" })]
    public async void OnAddPlayerGroup(ServerPlayer player, string expectedPlayerId, string expectedGroupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }


        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        var success = await _groupModule.Invite(null, group, target);
        if (success)
        {
            var definedJob = await _definedJobService.Find(j => j.CharacterModelId == player.CharacterModel.Id);
            if (definedJob != null)
            {
                await _definedJobService.Remove(definedJob);
            }
        }

        player.SendNotification(
            $"Du hast erfolgreich {target.CharacterModel.Name} der Gruppe {group.Name} hinzugefügt.",
            NotificationType.SUCCESS);
    }

    [Command("removeplayergroup", "Entferne einen Spieler aus einer Gruppe.", Permission.ADMIN,
        new[] { "Spieler ID", "Gruppen ID" })]
    public async void OnRemovePlayerGroup(ServerPlayer player, string expectedPlayerId, string expectedGroupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        await _groupModule.AdminKick(target, player, groupId);

        player.SendNotification(
            $"Du hast erfolgreich {target.CharacterModel.Name} aus der Gruppe {group.Name} entfernt.",
            NotificationType.SUCCESS);
    }

    #endregion

    #region Lease Company

    [Command("createcompany", "Erstelle an deiner aktuellen Position ein pachtbaren Unternehmenssitz.",
        Permission.HEAD_ECONOMY_MANAGEMENT, new[] { "Type", "Preis", "Name (Kein Name = 'FREI')" },
        CommandArgs.GREEDY_BUT_WITH_TWO_FIXED_ARGUMENT, new[] { "cc" })]
    public async void OnCreateLeaseCompany(ServerPlayer player, string expectedType, string expectedPrice,
        string expectedName)
    {
        if (!player.Exists)
        {
            return;
        }

        if (int.TryParse(expectedType, out var _))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedType, true, out LeaseCompanyType type))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPrice, out var price))
        {
            player.SendNotification("Keine richtige Zahl als Preis angegeben.", NotificationType.ERROR);
            return;
        }

        if (price < 0)
        {
            player.SendNotification("Der Preis darf nicht negativ sein.", NotificationType.ERROR);
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Du kannst in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (player.Dimension != 0)
        {
            player.SendNotification("Du kannst in keinem Interior oder Dimension sein.", NotificationType.ERROR);
            return;
        }

        await _houseModule.CreateLeaseCompany(type,
            new Position(player.Position.X, player.Position.Y, player.Position.Z - 1), player.Rotation, price,
            expectedName);

        player.SendNotification("Du hast ein pachtbaren Unternehmenssitz erstellt.", NotificationType.SUCCESS);
    }

    [Command("sethousegroupowner", "Setze eine bestimmte Gruppe als Eigentümer des Hauses.", Permission.ADMIN,
        new[] { "Gruppen ID", "Haus ID" })]
    public async void OnSetHouseGroupOwner(ServerPlayer player, string expectedPlayerId, string expectedLeaseCompanyId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var groupId))
        {
            player.SendNotification("Keine richtige Zahl als Gruppen ID angegeben.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedLeaseCompanyId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(houseId);
        if (house == null)
        {
            player.SendNotification("Es wurde kein pachtbares Unternehmen gefunden.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        house.GroupModelId = group.Id;

        await _houseService.Update(house);

        player.SendNotification($"Du hast {group.Name} als Eigentümer des Hauses gesetzt.", NotificationType.SUCCESS);
    }

    [Command("setcompanytype", "Ändere den Type welcher das Unternehmen hat.", Permission.ADMIN,
        new[] { "Haus ID", "Type" })]
    public async void OnSetLeaseCompanyType(ServerPlayer player, string expectedLeaseCompanyId, string expectedType)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedLeaseCompanyId, out var leaseCompanyId))
        {
            player.SendNotification("Keine richtige Zahl als pachtbarer Unternehmenssitz ID angegeben.",
                NotificationType.ERROR);
            return;
        }


        if (int.TryParse(expectedType, out var _))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedType, true, out LeaseCompanyType type))
        {
            player.SendNotification("Es konnte kein Typ gefunden werden.", NotificationType.ERROR);
            return;
        }

        if (await _houseService.GetByKey(leaseCompanyId) is not LeaseCompanyHouseModel leaseCompany)
        {
            player.SendNotification("Es wurde kein pachtbares Unternehmen gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.SetType(leaseCompany, type);

        player.SendNotification("Type wurde angepasst.", NotificationType.SUCCESS);
    }

    [Command("sethouserentable", "Stell ein ob ein Haus gemietet werden muss.", Permission.ECONOMY_MANAGEMENT,
        new[] { "Haus ID", "Status (Ja, Nein)" })]
    public async void OnSetHouseRentable(ServerPlayer player, string expectedHouseId, string expectedState)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var state = expectedState.ToLower() == "ja";
        var house = await _houseService.GetByKey(houseId);

        if (house == null)
        {
            player.SendNotification("Es wurde keine Immobilie gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.SetRentable(house, state);

        player.SendNotification("Type wurde angepasst.", NotificationType.SUCCESS);
    }

    [Command("sethouseblocked", "Blockiert den Besitztum des Hauses.", Permission.HEAD_ECONOMY_MANAGEMENT,
        new[] { "Haus ID", "Status (Ja, Nein)" })]
    public async void OnSetHouseBlocked(ServerPlayer player, string expectedHouseId, string expectedState)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedHouseId, out var houseId))
        {
            player.SendNotification("Keine richtige Zahl als Haus ID angegeben.", NotificationType.ERROR);
            return;
        }

        var state = expectedState.ToLower() == "ja";
        var house = await _houseService.GetByKey(houseId);

        if (house == null)
        {
            player.SendNotification("Es wurde keine Immobilie gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.SetBlockedOwnerShip(house, state);

        player.SendNotification("Besitztum wurde angepasst.", NotificationType.SUCCESS);
    }

    [Command("setcompanycashier", "Setze den Kassierer eines Unternehmen auf deine aktuelle Position.", Permission.MOD,
        new[] { "Haus ID" })]
    public async void OnSetLeaseCompanyCashier(ServerPlayer player, string expectedLeaseCompanyId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedLeaseCompanyId, out var leaseCompanyId))
        {
            player.SendNotification("Keine richtige Zahl als pachtbarer Unternehmenssitz ID angegeben.",
                NotificationType.ERROR);
            return;
        }

        if (await _houseService.GetByKey(leaseCompanyId) is not LeaseCompanyHouseModel leaseCompany)
        {
            player.SendNotification("Es wurde kein pachtbares Unternehmen gefunden.", NotificationType.ERROR);
            return;
        }

        DegreeRotation degreeRotation = player.Rotation;
        await _houseModule.SetCashier(leaseCompany,
            new Position(player.Position.X, player.Position.Y, player.Position.Z - 1), degreeRotation.Yaw);

        player.SendNotification("Kassierer wurde gesetzt.", NotificationType.SUCCESS);
    }

    [Command("removecompanycashier", "Entferne den Kassierer eines Unternehmens.", Permission.MOD, new[] { "Haus ID" })]
    public async void OnRemoveLeaseCompanyCashier(ServerPlayer player, string expectedLeaseCompanyId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedLeaseCompanyId, out var leaseCompanyId))
        {
            player.SendNotification("Keine richtige Zahl als pachtbarer Unternehmenssitz ID angegeben.",
                NotificationType.ERROR);
            return;
        }

        if (await _houseService.GetByKey(leaseCompanyId) is not LeaseCompanyHouseModel leaseCompany)
        {
            player.SendNotification("Es wurde kein pachtbares Unternehmen gefunden.", NotificationType.ERROR);
            return;
        }

        await _houseModule.ClearCashier(leaseCompany);

        player.SendNotification("Kassierer wurde entfernt.", NotificationType.SUCCESS);
    }

    #endregion

    #region Fun

    [Command("setplayermodel", "Setze das Player Model.", Permission.STAFF, new[] { "Spieler ID", "Model Hash" })]
    public async void OnSetPlayerModel(ServerPlayer player, string expectedPlayerId, string expectedModelHash)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!Enum.TryParse(expectedModelHash, out PedModel model))
        {
            player.SendNotification("Keine richtige Zahl als Model angegeben.", NotificationType.ERROR);
            return;
        }

        await target.SetModelAsync((uint)model);

        player.SendNotification("Model temporär angepasst.", NotificationType.INFO);
    }

    [Command("clearplayermodel", "Resette das Player Model.", Permission.STAFF, new[] { "Spieler ID" })]
    public async void OnClearPlayerModel(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        await _characterSpawnModule.Spawn(target, target.Position, target.Rotation, target.Dimension);

        target.SendNotification("Model zurückgesetzt.", NotificationType.INFO);
    }

    #endregion

    #region Animations

    [Command("testanim", "Teste eine Animation aus.", Permission.MANAGE_ANIMATIONS, new[] { "Dictionary", "Clip" })]
    public async void OnTestAnimation(ServerPlayer player, string expectedDictionary, string expectedClip)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            _animationModule.PlayAnimation(player, expectedDictionary, expectedClip);

            player.SendNotification("Test Animation wird abgespielt.", NotificationType.INFO);
        });
    }

    [Command("addanim", "Füge eine Animation zum Catalog hinzu.", Permission.MANAGE_ANIMATIONS,
        new[] { "Dictionary", "Clip", "Name" }, CommandArgs.GREEDY_BUT_WITH_TWO_FIXED_ARGUMENT)]
    public async void OnAddAnimation(ServerPlayer player, string expectedDictionary, string expectedClip,
        string expectedName)
    {
        if (!player.Exists)
        {
            return;
        }

        await _animationService.Add(new AnimationModel(expectedName, expectedDictionary, expectedClip));
        player.SendNotification("Du hast eine neue Animation hinzugefügt.", NotificationType.INFO);
    }

    [Command("deleteanim", "Entferne eine Animation vom Catalog.", Permission.MANAGE_ANIMATIONS,
        new[] { "Animation ID" })]
    public async void OnDeleteAnimation(ServerPlayer player, string expectedId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedId, out var animId))
        {
            player.SendNotification("Bitte gebe eine gültige ID an.", NotificationType.ERROR);
            return;
        }

        var animation = await _animationService.GetByKey(animId);
        if (animation == null)
        {
            player.SendNotification("Es konnte keine Animation gefunden werden.", NotificationType.ERROR);
            return;
        }

        await _animationService.Remove(animation);
        player.SendNotification("Du hast eine Animation entfernt.", NotificationType.INFO);
    }

    [Command("addanimflag", "Füge eine Flag zu einer Animation hinzu.", Permission.MANAGE_ANIMATIONS,
        new[] { "Animation ID", "Flag" })]
    public async void OnAddAnimationFlag(ServerPlayer player, string expectedId, string expectedFlag)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedId, out var animId))
        {
            player.SendNotification("Bitte gebe eine gültige ID an.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedFlag, out AnimationFlag flag))
        {
            player.SendNotification("Es wurde keine Flag unter diesen Namen gefunden.", NotificationType.ERROR);
            return;
        }

        var animation = await _animationService.GetByKey(animId);
        if (animation == null)
        {
            player.SendNotification("Es konnte keine Animation gefunden werden.", NotificationType.ERROR);
            return;
        }

        animation.Flags |= flag;

        await _animationService.Update(animation);
        player.SendNotification("Du hast eine Animation bearbeitet.", NotificationType.INFO);
    }

    [Command("removeanimflag", "Entferne eine Flag zu einer Animation hinzu.", Permission.MANAGE_ANIMATIONS,
        new[] { "Animation ID", "Flag" })]
    public async void OnRemoveAnimationFlag(ServerPlayer player, string expectedId, string expectedFlag)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedId, out var animId))
        {
            player.SendNotification("Bitte gebe eine gültige ID an.", NotificationType.ERROR);
            return;
        }

        if (!Enum.TryParse(expectedFlag, out AnimationFlag flag))
        {
            player.SendNotification("Es wurde keine Flag unter diesen Namen gefunden.", NotificationType.ERROR);
            return;
        }

        var animation = await _animationService.GetByKey(animId);
        if (animation == null)
        {
            player.SendNotification("Es konnte keine Animation gefunden werden.", NotificationType.ERROR);
            return;
        }

        animation.Flags &= ~ flag;

        await _animationService.Update(animation);
        player.SendNotification("Du hast eine Animation bearbeitet.", NotificationType.INFO);
    }

    [Command("renameanim", "Animation umbenennen", Permission.MANAGE_ANIMATIONS, new[] { "Animation ID", "Neuer Name" },
        CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnRenameAnimationFlag(ServerPlayer player, string expectedId, string expectedName)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedId, out var animId))
        {
            player.SendNotification("Bitte gebe eine gültige ID an.", NotificationType.ERROR);
            return;
        }

        var animation = await _animationService.GetByKey(animId);
        if (animation == null)
        {
            player.SendNotification("Es konnte keine Animation gefunden werden.", NotificationType.ERROR);
            return;
        }

        animation.Name = expectedName;

        await _animationService.Update(animation);
        player.SendNotification("Du hast eine Animation umbenannt.", NotificationType.INFO);
    }

    #endregion

    #region Punishment

    [Command("setaprison", "Setze den Spieler ins Admin Prison.", Permission.STAFF,
        new[] { "Spieler ID", "Anzahl der Checkpoints", "Grund" }, CommandArgs.GREEDY_BUT_WITH_TWO_FIXED_ARGUMENT)]
    public async void OnSetAdminPrison(ServerPlayer player, string expectedPlayerId, string expectedCheckpointsAmount,
        string reason)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (!int.TryParse(expectedCheckpointsAmount, out var checkpointsAmount))
        {
            player.SendNotification("Keine richtige Zahl als Checkpoints angegeben.", NotificationType.ERROR);
            return;
        }

        // if (target == player)
        // {
        //     player.SendNotification("Du kannst den Befehl nicht auf dich selbst anwenden.", NotificationType.ERROR);
        //     return;
        // }

        // Just don't ask questions...
        if (target.IsAduty)
        {
            target.IsAduty = false;
        }

        if (target.AccountModel.AdminCheckpoints == 0)
        {
            await _characterService.Update(target);
        }

        target.AccountModel.AdminCheckpoints = checkpointsAmount;

        await _adminPrisonModule.SetPlayerInPrison(target, player, reason);

        await _accountService.Update(target.AccountModel);

        player.SendNotification(
            $"Du hast den Spieler {target.AccountName} für {checkpointsAmount} Checkpoints in das Admin Prison gesteckt.",
            NotificationType.INFO);
    }

    [Command("clearaprison", "Befreie den Spieler aus dem Admin Prison.", Permission.STAFF, new[] { "Spieler ID" })]
    public async void OnClearAdminPrison(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (target == player)
        {
            player.SendNotification("Du kannst den Befehl nicht auf dich selbst anwenden.", NotificationType.ERROR);
            return;
        }

        if (target.AccountModel.AdminCheckpoints == 0)
        {
            player.SendNotification("Der Spieler befindet sich nicht im Admin Prison.", NotificationType.ERROR);
            return;
        }

        target.AccountModel.AdminCheckpoints = 0;

        await _adminPrisonModule.ClearPlayerFromPrison(target, player);
        await _accountService.Update(target.AccountModel);

        player.SendNotification($"Du hast den Spieler {target.AccountName} aus dem Admin Prison befreit.",
            NotificationType.INFO);
    }

    [Command("kick", "Kicke den Spieler vom Server.", Permission.STAFF, new[] { "Spieler ID", "Grund" },
        CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnAdminKick(ServerPlayer player, string expectedPlayerId, string expectedReason)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }


        if (target == player)
        {
            player.SendNotification("Du kannst den Befehl nicht auf dich selbst anwenden.", NotificationType.ERROR);
            return;
        }

        await _characterService.Update(target);
        await _accountService.Update(target.AccountModel);

        await _userRecordLogService.Add(new UserRecordLogModel
        {
            AccountModelId = target.AccountModel.SocialClubId,
            StaffAccountModelId = player.AccountModel.SocialClubId,
            CharacterModelId = target.CharacterModel.Id,
            UserRecordType = UserRecordType.AUTOMATIC,
            Text = "Spieler wurde mit dem Grund '" + expectedReason + "' vom Server gekickt."
        });

        await target.KickAsync($"Du wurdest von {player.AccountName} gekickt! Grund: {expectedReason}");

        player.SendNotification($"Du hast den Spieler {target.AccountName} für den Grund: '{expectedReason}' gekickt.",
            NotificationType.INFO);
    }

    [Command("timeban", "Banne den Spieler temporär vom Server.", Permission.STAFF,
        new[] { "Spieler ID", "Dauer in Stunden", "Grund" }, CommandArgs.GREEDY_BUT_WITH_TWO_FIXED_ARGUMENT)]
    public async void OnAdminTimeBan(ServerPlayer player, string expectedPlayerId, string expectedDuration,
        string expectedReason)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (target == player)
        {
            player.SendNotification("Du kannst den Befehl nicht auf dich selbst anwenden.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedDuration, out var hours))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl als Dauer in Stunden an.", NotificationType.ERROR);
        }

        target.AccountModel.BannedFrom = player.DiscordId;
        target.AccountModel.BannedReason = expectedReason;
        target.AccountModel.BannedUntil = DateTime.Now.AddHours(hours + 2); // cause timezone

        await _characterService.Update(target);
        await _accountService.Update(target.AccountModel);

        await _userRecordLogService.Add(new UserRecordLogModel
        {
            AccountModelId = target.AccountModel.SocialClubId,
            StaffAccountModelId = player.AccountModel.SocialClubId,
            CharacterModelId = target.CharacterModel.Id,
            UserRecordType = UserRecordType.AUTOMATIC,
            Text = "Spieler wurde mit dem Grund '" + expectedReason + "' für '" + hours +
                   "' Stunden temporär vom Server ausgeschlossen."
        });

        await target.KickAsync("Du wurdest temporär von unserer Community ausgeschlossen!\n" +
                               $"Grund: {target.AccountModel.BannedReason}\n\n" +
                               $"Ablauf: {target.AccountModel.BannedUntil:HH:mm:ss dd.MM.yyyy}.");

        player.SendNotification(
            $"Du hast den Spieler {target.AccountName} für den Grund: '{expectedReason}' '{hours}' Stunde/n temporär ausgeschlossen.",
            NotificationType.INFO);
    }

    [Command("ban", "Banne den Spieler permanent vom Server.", Permission.ADMIN, new[] { "Spieler ID", "Grund" },
        CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnAdminBan(ServerPlayer player, string expectedPlayerId, string expectedReason)
    {
        if (!player.Exists)
        {
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }

        if (target == player)
        {
            player.SendNotification("Du kannst den Befehl nicht auf dich selbst anwenden.", NotificationType.ERROR);
            return;
        }

        target.AccountModel.BannedFrom = player.DiscordId;
        target.AccountModel.BannedReason = expectedReason;
        target.AccountModel.BannedPermanent = true;

        await _characterService.Update(target);
        await _accountService.Update(target.AccountModel);

        await _userRecordLogService.Add(new UserRecordLogModel
        {
            AccountModelId = target.AccountModel.SocialClubId,
            StaffAccountModelId = player.AccountModel.SocialClubId,
            CharacterModelId = target.CharacterModel.Id,
            UserRecordType = UserRecordType.AUTOMATIC,
            Text = "Spieler wurde mit dem Grund '" + expectedReason + "' vom Server ausgeschlossen."
        });

        await target.KickAsync(
            $"Du wurdest von unserer Community ausgeschlossen! Grund: {target.AccountModel.BannedReason}");

        player.SendNotification(
            $"Du hast den Spieler {target.AccountName} für den Grund: '{expectedReason}' permanent ausgeschlossen.",
            NotificationType.INFO);
    }

    [Command("unban", "Entbanne den Spieler vom Server.", Permission.ADMIN, new[] { "Discord ID", "Grund" },
        CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnAdminUnban(ServerPlayer player, string expectedDiscordId, string expectedReason)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!ulong.TryParse(expectedDiscordId, out var discordId))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl als Discord ID an.", NotificationType.ERROR);
        }

        var bannedAccount = await _accountService.Find(a => a.DiscordId == discordId);
        if (bannedAccount == null)
        {
            player.SendNotification("Es konnte kein Account mit der Discord ID gefunden werden.",
                NotificationType.ERROR);
            return;
        }

        bannedAccount.BannedFrom = 0;
        bannedAccount.BannedReason = null;
        bannedAccount.BannedUntil = DateTime.MinValue;
        bannedAccount.BannedPermanent = false;

        await _userRecordLogService.Add(new UserRecordLogModel
        {
            AccountModelId = bannedAccount.SocialClubId,
            StaffAccountModelId = player.AccountModel.SocialClubId,
            UserRecordType = UserRecordType.AUTOMATIC,
            Text = "Spieler wurde mit dem Grund '" + expectedReason + "' entbannt."
        });

        await _accountService.Update(bannedAccount);

        player.SendNotification($"Du hast den Spieler mit der Discord ID {bannedAccount.DiscordId} wieder entbannt.",
            NotificationType.INFO);
    }

    #endregion
}