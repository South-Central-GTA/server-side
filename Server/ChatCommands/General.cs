using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Housing;
using Server.Database.Models.Inventory;
using Server.Modules.Admin;
using Server.Modules.Animation;
using Server.Modules.Bank;
using Server.Modules.Character;
using Server.Modules.Chat;
using Server.Modules.Delivery;
using Server.Modules.Discord;
using Server.Modules.EntitySync;
using Server.Modules.Group;
using Server.Modules.Houses;
using Server.Modules.Inventory;
using Server.Modules.Key;
using Server.Modules.RoleplayInfo;
using Server.Modules.Vehicles;

namespace Server.ChatCommands;

public class General : ISingletonScript
{
    private readonly AnimationModule _animationModule;
    private readonly BankModule _bankModule;
    private readonly CharacterSelectionModule _characterSelectionModule;
    private readonly CharacterService _characterService;
    private readonly ChatModule _chatModule;
    private readonly DeliveryModule _deliveryModule;
    private readonly DiscordModule _discordModule;
    private readonly GroupFactionService _groupFactionService;
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly HelpMeModule _helpMeModule;
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly InventoryModule _inventoryModule;
    private readonly InventoryService _inventoryService;
    private readonly ItemCatalogService _itemCatalogService;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ItemService _itemService;

    private readonly LockModule _lockModule;
    private readonly PedSyncModule _pedSyncModule;
    private readonly RegistrationOfficeService _registrationOfficeService;
    private readonly RoleplayInfoModule _roleplayInfoModule;
    private readonly VehicleModule _vehicleModule;

    public General(HouseService houseService, CharacterService characterService, ItemService itemService,
        GroupService groupService, GroupFactionService groupFactionService, ItemCatalogService itemCatalogService,
        InventoryService inventoryService, LockModule lockModule, ItemCreationModule itemCreationModule,
        InventoryModule inventoryModule, HouseModule houseModule, VehicleModule vehicleModule, BankModule bankModule,
        GroupModule groupModule, DeliveryModule deliveryModule, ChatModule chatModule, PedSyncModule pedSyncModule,
        HelpMeModule helpMeModule, DiscordModule discordModule, RoleplayInfoModule roleplayInfoModule,
        AnimationModule animationModule, RegistrationOfficeService registrationOfficeService,
        CharacterSelectionModule characterSelectionModule)
    {
        _houseService = houseService;
        _characterService = characterService;
        _itemService = itemService;
        _groupService = groupService;
        _groupFactionService = groupFactionService;
        _itemCatalogService = itemCatalogService;
        _inventoryService = inventoryService;

        _lockModule = lockModule;
        _itemCreationModule = itemCreationModule;
        _inventoryModule = inventoryModule;
        _houseModule = houseModule;
        _vehicleModule = vehicleModule;
        _bankModule = bankModule;
        _groupModule = groupModule;
        _deliveryModule = deliveryModule;
        _chatModule = chatModule;
        _pedSyncModule = pedSyncModule;
        _helpMeModule = helpMeModule;
        _discordModule = discordModule;
        _roleplayInfoModule = roleplayInfoModule;
        _animationModule = animationModule;
        _registrationOfficeService = registrationOfficeService;
        _characterSelectionModule = characterSelectionModule;
    }

    [Command("verify", "Update deine Berechtigungen basierend auf unseren Discord Server.")]
    public async void OnVerify(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await _discordModule.UpdatePermissions(player);
        player.SendNotification("Deine Berechtigungen wurden erfolgreich geupdated.", NotificationType.SUCCESS);
    }

    [Command("cuff", "Lege einem Charakter Handschellen an wenn dein Charakter das Item im Inventar hat.",
        Permission.NONE, new[] { "Spieler ID" })]
    public async void OnCuff(ServerPlayer player, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        var itemHandCuff =
            (ItemHandCuffModel)player.CharacterModel.InventoryModel.Items.FirstOrDefault(i =>
                i.CatalogItemModelId == ItemCatalogIds.HANDCUFF);
        if (itemHandCuff == null)
        {
            player.SendNotification("Dein Charakter hat keine Handschellen im Inventar.", NotificationType.ERROR);
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

        if (player.Position.Distance(target.Position) > 2)
        {
            player.SendNotification($"Dein Charakter ist von {target.CharacterModel.Name} zu weit entfernt.",
                NotificationType.ERROR);
            return;
        }

        if (target.CharacterModel.InventoryModel.Items.Any(i =>
                i.CatalogItemModelId == ItemCatalogIds.HANDCUFF && i.ItemState == ItemState.FORCE_EQUIPPED))
        {
            player.SendNotification("Der Charakter hat schon Handschellen an.", NotificationType.ERROR);
            return;
        }

        var oldHandcuffAmount = itemHandCuff.Amount;

        itemHandCuff.ItemKeyModelId = null;
        itemHandCuff.GroupModelId = null;

        if (!await _groupModule.IsPlayerInGroupType(player, GroupType.FACTION))
        {
            var keyItem = await _itemCreationModule.AddItemAsync(player.CharacterModel.InventoryModel,
                ItemCatalogIds.HANDCUFF_KEY, 1);
            if (keyItem == null)
            {
                player.SendNotification(
                    "Dein Charakter hatte kein Platz in seinem Inventar für den Schlüssel der Handschellen.",
                    NotificationType.ERROR);
                return;
            }

            itemHandCuff.ItemKeyModelId = keyItem.Id;
        }
        else
        {
            var faction = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
            if (faction == null)
            {
                return;
            }

            target.SendNotification("Jeder aus der Fraktion mit einem Gruppenschlüssel kann die Handschellen abnehmen.",
                NotificationType.INFO);

            itemHandCuff.GroupModelId = faction.Id;
        }

        if (oldHandcuffAmount > 1)
        {
            itemHandCuff.Amount = 1;
        }

        itemHandCuff.Slot = null;
        itemHandCuff.InventoryModelId = target.CharacterModel.InventoryModel.Id;
        itemHandCuff.ItemState = ItemState.FORCE_EQUIPPED;

        await _itemService.Update(itemHandCuff);

        // Give the player the amount of handcuffs back.
        if (oldHandcuffAmount > 1)
        {
            await _itemCreationModule.AddItemAsync(player.CharacterModel.InventoryModel, ItemCatalogIds.HANDCUFF,
                oldHandcuffAmount - 1);
        }

        target.Cuffed = true;

        await _inventoryModule.UpdateInventoryUiAsync(player);
        await _inventoryModule.UpdateInventoryUiAsync(target);

        target.SendNotification($"Deinem Charakter wurden von {player.CharacterModel.Name} Handschellen angelegt.",
            NotificationType.INFO);
        player.SendNotification($"Dein Charakter hat {target.CharacterModel.Name} Handschellen angelegt.",
            NotificationType.SUCCESS);
    }



    [Command("addinfo", "Erstelle eine Info, um etwas zu beschreiben.", Permission.NONE, new[] { "Distanz", "Text" },
        CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT)]
    public async void OnAddInfo(ServerPlayer player, string expectedDistance, string expectedInfo)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Du kannst in einem Fahrzeug keine Info erstellen.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedDistance, out var distance))
        {
            player.SendNotification("Bitte gebe eine positive Zahl an.", NotificationType.ERROR);
            return;
        }

        if (distance < 1)
        {
            player.SendNotification("Die Distanz ist zu kurz.", NotificationType.ERROR);
            return;
        }

        if (distance > 30)
        {
            player.SendNotification("Maximale Distanz ist 30 Einheiten.", NotificationType.ERROR);
            return;
        }

        if (expectedInfo.Length <= 5)
        {
            player.SendNotification("Deine Info ist zu kurz.", NotificationType.ERROR);
            return;
        }

        if (expectedInfo.Length >= 256)
        {
            player.SendNotification("Deine Info ist zu lang 256 Zeichenlimit.", NotificationType.ERROR);
            return;
        }

        if (await _roleplayInfoModule.ReachedLimit(player))
        {
            player.SendNotification("Du kannst nicht mehr Infos platzieren.", NotificationType.ERROR);
            return;
        }

        await _roleplayInfoModule.AddInfo(player, distance, expectedInfo);
    }

    [Command("deleteinfo", "Entferne eine Info.")]
    public async void OnDeleteInfo(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await _roleplayInfoModule.DeleteInfo(player);
    }

    [Command("stopanim", "Stopt deine aktuelle Animation.")]
    public async void OnStopAnimation(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            _animationModule.ClearAnimation(player);

            player.SendNotification("Animation wurde gestoppt.", NotificationType.INFO);
        });
    }

    [Command("helpme", "Erkläre dein Problem in einer kurzen Nachricht.", Permission.NONE,
        new[] { "Beschreibung des Problems" }, CommandArgs.GREEDY)]
    public async void OnHelpMe(ServerPlayer player, string message)
    {
        await AltAsync.Do(() =>
        {
            message = message.TrimEnd().TrimStart();

            if (message.Length == 0)
            {
                player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
                return;
            }

            _helpMeModule.CreateTicket(player, message);
        });
    }

    [Command("delhelpme", "Lösche dein letztes Ticket.", Permission.NONE, null, CommandArgs.GREEDY,
        new[] { "deletehelpme", "removehelpticket" })]
    public async void OnDeleteHelpMe(ServerPlayer player)
    {
        await AltAsync.Do(() => { _helpMeModule.DeleteTicket(player); });
    }

    [Command("id", "Zeige deine eigene Id an.")]
    public async void OnGetId(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            player.SendNotification("ID: " + player.Id, NotificationType.INFO);
        });
    }

    [Command("lock", "Schließt ein Schloss auf oder zu.")]
    public async void OnLock(ServerPlayer player)
    {
        var entity = await _lockModule.GetClosestLockableEntity(player);

        if (entity == null)
        {
            player.SendNotification("Es befindet sich kein Schloss in der Nähe.", NotificationType.ERROR);
            return;
        }

        await _lockModule.Lock(player, entity);
    }

    [Command("deletewaypoint", "Lösche deinen aktuellen Marker auf der Karte.", Permission.NONE, null,
        CommandArgs.NOT_GREEDY, new[] { "dw" })]
    public async void OnDeleteWaypoint(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            lock (player)
            {
                player.ClearWaypoint();
                player.SendNotification("Du hast das deinen Marker entfernt.", NotificationType.SUCCESS);
            }
        });
    }

    [Command("pay", "Übergebe einen anderen Character Geld.", Permission.NONE,
        new[] { "Spieler ID", "Anzahl", "Emote" }, CommandArgs.GREEDY_BUT_WITH_TWO_FIXED_ARGUMENT)]
    public async void OnPay(ServerPlayer player, string expectedPlayerId, string expectedAmount, string expectedEmote)
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

        if (!int.TryParse(expectedAmount, out var amount))
        {
            player.SendNotification("Bitte gebe eine gültige Menge an.", NotificationType.ERROR);
            return;
        }

        if (expectedEmote.Length == 0)
        {
            player.SendNotification("Bitte gebe eine Emote an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, playerId);
        if (target == null)
        {
            return;
        }

        if (target == player)
        {
            player.SendNotification("Du kannst nicht deinen eigenen Charakter Geld geben.", NotificationType.ERROR);
            return;
        }

        if (target.Position.Distance(player.Position) > 5)
        {
            player.SendNotification("Die Entfernung ist zu groß.", NotificationType.ERROR);
            return;
        }

        var amountOfDollar = _inventoryModule.AmountOfItem(player.CharacterModel.InventoryModel, ItemCatalogIds.DOLLAR);
        if (amountOfDollar == null)
        {
            player.SendNotification("Es wurde kein Geld in dem Inventar deines Charakters gefunden.",
                NotificationType.ERROR);
            return;
        }

        if (amount < 1)
        {
            player.SendNotification("Du musst jemanden min. 1$ geben", NotificationType.ERROR);
            return;
        }

        if (amount > amountOfDollar)
        {
            player.SendNotification("Dein Charakter hat nicht genug Geld dabei.", NotificationType.ERROR);
            return;
        }

        var newAddedItem = await _itemCreationModule.AddItemAsync(target, ItemCatalogIds.DOLLAR, amount);
        if (newAddedItem == null)
        {
            player.SendNotification($"{target.CharacterModel.Name} hat kein Platz im Inventar.",
                NotificationType.ERROR);
            return;
        }

        var toRemove = amount;
        player.CharacterModel.InventoryModel =
            await _inventoryService.GetByKey(player.CharacterModel.InventoryModel.Id);
        foreach (var item in player.CharacterModel.InventoryModel.Items.Where(i =>
                     i.CatalogItemModelId == ItemCatalogIds.DOLLAR))
        {
            if (item.Amount - toRemove <= 0)
            {
                toRemove -= item.Amount;
                await _itemService.Remove(item);
            }
            else
            {
                item.Amount -= toRemove;
                await _itemService.Update(item);
                break;
            }
        }

        await _inventoryModule.UpdateInventoryUiAsync(player);

        await _chatModule.SendProxMessage(player, 15, ChatType.EMOTE, expectedEmote);

        player.SendNotification($"Dein Charakter hat {target.CharacterModel.Name} {amount} Dollar gegeben.",
            NotificationType.SUCCESS);
        target.SendNotification($"Dein Charakter hat von {player.CharacterModel.Name} {amount} Dollar erhalten.",
            NotificationType.INFO);
    }

    [Command("settorso", "Passe den Torso deines Charakters an.")]
    public async void OnSetTorso(ServerPlayer player)
    {
        await AltAsync.Do(() =>
        {
            if (!player.Exists)
            {
                return;
            }

            player.EmitLocked("settorsomenu:show");
        });
    }

    [Command("engine", "Schalte den Motor eines Fahrzeuges an oder aus.", Permission.NONE, null, CommandArgs.NOT_GREEDY,
        new[] { "e", "motor" })]
    public async void OnEngine(ServerPlayer player)
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

        if (player.Seat != 1)
        {
            var gender = player.CharacterModel.Gender == GenderType.MALE ? "der Fahrer" : "die Fahrerin";
            player.SendNotification($"Dein Charakter muss {gender} des Fahrzeug sein.", NotificationType.ERROR);
            return;
        }

        await _vehicleModule.SetEngineState((ServerVehicle)player.Vehicle, player, !player.Vehicle.EngineOn);
    }

    [Command("buyhouse", "Öffne ein Kaufdialog für das Haus an welchem du gerade stehst.")]
    public async void OnBuyHouse(ServerPlayer player)
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

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.KEY))
        {
            return;
        }

        var house = await _houseService.GetByDistance(player.Position);
        if (house == null || house.HouseType != HouseType.HOUSE)
        {
            player.SendNotification("Es ist kein Haus in der Nähe deines Charakters.", NotificationType.ERROR);
            return;
        }

        if (house.BlockedOwnership)
        {
            return;
        }

        if (house.HasOwner)
        {
            player.SendNotification("Dieses Haus hat schon einen Eigentümer.", NotificationType.ERROR);
            return;
        }

        if (_houseModule.IsHouseBlocked(house.Id))
        {
            player.SendNotification(
                "Dieses Haus wurde sich in der Charaktererstellung vorgemerkt und kann aktuell nicht gekauft werden.",
                NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Dein Charakter benötigt ein Bankkonto um ein Haus zu kaufen.",
                NotificationType.ERROR);
            return;
        }

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Haus kaufen",
            Description =
                $"Möchtest du dieses Haus für <b>${house.Price}</b> erwerben?<br><br><span class='text-muted'>Die Kosten werden von deinem angegebenen Bankkonto abgezogen.</span>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            PrimaryButton = "Kaufen",
            PrimaryButtonServerEvent = "housedialog:buy"
        });
    }

    [Command("renthouse", "Öffne ein Dialog um eine mietbare Immobilie zu mieten.")]
    public async void OnRentHouse(ServerPlayer player)
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

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            player.SendNotification("Dein Charakter ist nicht im Registration Office gemeldet.",
                NotificationType.ERROR);
            return;
        }

        if (!await _inventoryModule.CanCarry(player, ItemCatalogIds.KEY))
        {
            return;
        }

        var house = await _houseService.GetByDistance(player.Position);
        if (house == null)
        {
            player.SendNotification("Es ist keine mietbare Immobilie in der Nähe.", NotificationType.ERROR);
            return;
        }

        if (house.BlockedOwnership)
        {
            return;
        }

        if (!house.Rentable)
        {
            player.SendNotification("Diese Immobilie ist nicht mietbar.", NotificationType.ERROR);
            return;
        }

        if (house.HasOwner)
        {
            player.SendNotification("Dieser pachtbare Unternehmenssitz hat schon einen Eigentümer.",
                NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Dein Charakter benötigt ein Bankkonto um eine Immobilie zu mieten.",
                NotificationType.ERROR);
            return;
        }

        switch (house.HouseType)
        {
            case HouseType.HOUSE:
                player.CreateDialog(new DialogData
                {
                    Type = DialogType.ONE_BUTTON_DIALOG,
                    Title = "Mietbare Immobilie",
                    Description =
                        $"Möchtest du diese Immobilie für <b>${house.Price}</b> pro Zahltag mieten?<br><br><span class='text-muted'>Die Kosten werden von deinem angegebenen Bankkonto jeden Zahltag automatisch abgezogen, sollte nicht genügend Geld auf dem Konto sein verliert dein Charakter die Immobilie.</span>",
                    HasBankAccountSelection = true,
                    FreezeGameControls = true,
                    PrimaryButton = "Mieten",
                    PrimaryButtonServerEvent = "house:rent"
                });
                break;
            case HouseType.COMPANY:
                var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
                var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY);
                if (group == null)
                {
                    player.SendNotification("Dein Charakter ist in keinem Unternehmen.", NotificationType.ERROR);
                    return;
                }

                if (!_groupModule.IsOwner(player, group))
                {
                    player.SendNotification("Dein Charakter ist nicht der Eigentümer des Unternehmens.",
                        NotificationType.ERROR);
                    return;
                }

                player.CreateDialog(new DialogData
                {
                    Type = DialogType.ONE_BUTTON_DIALOG,
                    Title = "Pachtbarer Unternehmenssitz pachten",
                    Description =
                        $"Möchtest du diesen pachtbaren Unternehmenssitz für <b>${house.Price}</b> pro Zahltag pachten?<br><br><span class='text-muted'>Die Kosten werden von deinem angegebenen Bankkonto jeden Zahltag automatisch abgezogen, sollte nicht genügend Geld auf dem Konto sein verliert dein Charakter den Unternehmenssitz.</span>",
                    HasBankAccountSelection = true,
                    FreezeGameControls = true,
                    PrimaryButton = "Pachten",
                    PrimaryButtonServerEvent = "company:lease"
                });
                break;
        }
    }

    [Command("unrenthouse", "Öffne ein Dialog um den Mietvertrag zu kündigen.")]
    public async void OnUnRentHouse(ServerPlayer player)
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

        var house = await _houseService.GetByDistance(player.Position);
        if (house == null)
        {
            player.SendNotification("Es ist keine mietbare Immobilie in der Nähe.", NotificationType.ERROR);
            return;
        }

        if (!house.Rentable)
        {
            player.SendNotification("Dies ist eine gekaufte Immobilie du kannst hier kein Mietvertrag kündigen.",
                NotificationType.ERROR);
            return;
        }

        if (!house.CharacterModelId.HasValue || house.CharacterModelId.Value != player.CharacterModel.Id)
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer der Immobilie.", NotificationType.ERROR);
            return;
        }

        if (house.GroupModelId.HasValue && house.HouseType != HouseType.COMPANY)
        {
            var group = await _groupService.GetByKey(house.GroupModelId.Value);
            if (group != null)
            {
                player.SendNotification(
                    $"Diese Immobilie hat das Unternehmen {group.Name} als Hauptsitz daher kann der Mietvertrag nicht gekündigt werden.",
                    NotificationType.ERROR);
            }

            return;
        }

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Mietvertrag kündigen",
            Description = "Willst du den Mietvertrag für diese Immobilie wirklich kündigen?",
            FreezeGameControls = true,
            PrimaryButton = "Ja",
            PrimaryButtonServerEvent = "house:unrent"
        });
    }

    [Command("sellhouse", "Öffne ein Verkaufsdialog für das Haus an welchem du gerade stehst.")]
    public async void OnSellHouse(ServerPlayer player)
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

        var house = await _houseService.GetByDistance(player.Position);
        if (house == null)
        {
            player.SendNotification("Es ist kein Haus in der Nähe deines Charakters.", NotificationType.ERROR);
            return;
        }

        if (!house.CharacterModelId.HasValue || house.CharacterModelId.Value != player.CharacterModel.Id)
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer der Immobilie.", NotificationType.ERROR);
            return;
        }

        if (house.GroupModelId.HasValue && house.HouseType != HouseType.COMPANY)
        {
            var group = await _groupService.GetByKey(house.GroupModelId.Value);
            if (group != null)
            {
                player.SendNotification(
                    $"Diese Immobilie hat das Unternehmen {group.Name} als Hauptsitz und kann daher nicht verkauft werden.",
                    NotificationType.ERROR);
            }

            return;
        }

        if (house.Rentable)
        {
            player.SendNotification("Dies ist eine gemietete Immobilie du kannst sie nicht verkaufen.",
                NotificationType.ERROR);
            return;
        }

        if (house.HouseType == HouseType.COMPANY)
        {
            player.SendNotification("Du musst den Pachtvertrag kündigen, dies kannst du per /unrenthouse machen.",
                NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Dein Charakter benötigt ein Bankkonto um eine Immobilie verkaufen zu können.",
                NotificationType.ERROR);
            return;
        }

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Haus verkaufen",
            Description =
                $"Möchtest du dieses Haus für <b>${house.Price * 0.8f}</b> verkaufen, dies sind 80% von dem Kaufpreis.<br><br><span class='text-muted'>Das Geld wird dann auf dein angegebenes Bankkonto überwiesen.</span>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            PrimaryButton = "Verkaufen",
            PrimaryButtonServerEvent = "housedialog:sell"
        });
    }

    [Command("enter", "Betrete ein Haus.")]
    public async void OnHouseEnter(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf nicht in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        await _houseModule.Enter(player);
    }

    [Command("exit", "Verlasse ein Haus.")]
    public async void OnHouseLeave(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification("Dein Charakter darf nicht in einem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        await _houseModule.Exit(player);
    }

    [Command("switchhlock", "Wechsel das Schloss des Hauses aus wenn du der Eigentümer bist.")]
    public async void OnSwitchHouseLock(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.Dimension == 0)
        {
            player.SendNotification("Du musst in einem Haus sein.", NotificationType.ERROR);
            return;
        }

        var house = await _houseService.GetByKey(player.Dimension);
        if (house.CharacterModelId != player.CharacterModel.Id)
        {
            player.SendNotification("Du musst der Eigentümer des Hauses sein.", NotificationType.ERROR);
            return;
        }

        if (!await _houseModule.CreateHouseKey(player.CharacterModel, house))
        {
            player.SendNotification("Dein Charakter hat nicht genug Platz im Inventar für den Hausschlüssel.",
                NotificationType.ERROR);
            return;
        }

        player.SendNotification("Du hast das Schloss ausgewechselt, spiele es nun im Roleplay aus.",
            NotificationType.SUCCESS);
    }

    [Command("changechar", "Gehe zurück in die Charakterauswahl.", Permission.NONE, null, CommandArgs.NOT_GREEDY,
        new[] { "q", "quit", "logout", "mc" })]
    public async void OnChangeChar(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await _characterService.Update(player);

        await _characterSelectionModule.OpenAsync(player);
    }

    #region Groups

    [Command("invite", "Lade ein Charakter in deine Gruppe.", Permission.NONE, new[] { "Gruppen ID", "Spieler ID" })]
    public async void OnInvite(ServerPlayer player, string expectedGroupId, string expectedPlayerId)
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

        var target = Alt.GetAllPlayers().GetPlayerById(player, playerId);
        if (target == null)
        {
            return;
        }

        if (target.IsInVehicle)
        {
            player.SendNotification("Der Charakter darf in keinem Fahrzeug sitzen.", NotificationType.ERROR);
            return;
        }

        if (player.Position.Distance(target.Position) > 3)
        {
            player.SendNotification("Der Charakter muss in deiner Nähe sein.", NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine gültige Gruppe Id an.", NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(target, GroupType.COMPANY))
        {
            player.SendNotification(
                "Der Charakter ist schon in einem spielerbasierten Unternehmen und kann deswegen nicht eingeladen werden.",
                NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(target, GroupType.FACTION))
        {
            player.SendNotification(
                "Der Charakter ist schon in einer Fraktion und kann deswegen nicht eingeladen werden.",
                NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.INVITE))
        {
            player.SendNotification("Dein Charakter hat dafür nicht genügend Berechtigungen in der Gruppe.",
                NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasBankAccount(target))
        {
            player.SendNotification("Der Charakter muss ein Bankkonto besitzen.", NotificationType.ERROR);
            return;
        }

        var data = new object[2];
        data[0] = group.Id;
        data[1] = player.Id;

        var typeString = "";

        switch (group.GroupType)
        {
            case GroupType.FACTION:
                typeString = "die Fraktion";
                break;
            case GroupType.COMPANY:
                typeString = "das Unternehmen";
                break;
        }

        lock (target)
        {
            target.CreateDialog(new DialogData
            {
                Type = DialogType.TWO_BUTTON_DIALOG,
                Title = "Einladung",
                Description =
                    $"{player.CharacterModel.Name} hat deinen Charakter in {typeString} <b>{group.Name}</b> eingeladen.<br><br>Solltest du annehmen, auf welches Bankkonto soll das Gehalt überwiesen werden?",
                HasBankAccountSelection = true,
                FreezeGameControls = true,
                Data = data,
                PrimaryButton = "Einladung annehmen",
                SecondaryButton = "Einladung ablehnen",
                PrimaryButtonServerEvent = "group:inviteaccept",
                SecondaryButtonServerEvent = "group:invitedeny",
                CloseButtonServerEvent = "group:invitedeny"
            });
        }

        player.SendNotification("Du hast dem Charakter eine Einladung geschickt.", NotificationType.INFO);
    }

    [Command("uninvite", "Werfe einen Charakter aus deiner Gruppe.", Permission.NONE,
        new[] { "Gruppen ID", "Spieler ID" })]
    public async void OnUninvite(ServerPlayer player, string expectedGroupId, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine gültige Gruppe Id an.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.UNINVITE))
        {
            player.SendNotification("Dein Charakter hat dafür nicht genügend Berechtigungen in der Gruppe.",
                NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe eine gültige Spieler ID an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, playerId);
        if (target == null)
        {
            return;
        }

        if (player == target)
        {
            player.SendNotification("Du kannst den Befehl nicht an dir selbst nutzen.", NotificationType.ERROR);
            return;
        }

        var groupMember = group.Members.Find(m => m.CharacterModelId == target.CharacterModel.Id);
        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Der Charakter ist der Eigentümer dieser Gruppe.", NotificationType.ERROR);
            return;
        }

        if (groupMember == null)
        {
            player.SendNotification("Der Charakter befindet sich nicht in deiner Gruppe.", NotificationType.ERROR);
            return;
        }

        await _groupModule.Kick(player, groupMember);
    }

    [Command("setsalary", "Setze das Gehalt von einem Mitglied aus deiner Gruppe.", Permission.NONE,
        new[] { "Gruppen ID", "Spieler ID", "Gehalt" })]
    public async void OnSetSalary(ServerPlayer player, string expectedGroupId, string expectedPlayerId,
        string expectedSalary)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine gültige Gruppe Id an.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.MANAGE_MEMBERS))
        {
            player.SendNotification("Dein Charakter hat dafür nicht genügend Berechtigungen in der Gruppe.",
                NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe eine gültige Spieler ID an.", NotificationType.ERROR);
            return;
        }

        if (!uint.TryParse(expectedSalary, out var salary))
        {
            player.SendNotification("Bitte gebe eine gültige Zahl als Gehalt an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, playerId);
        if (target == null)
        {
            return;
        }

        await _groupModule.SetSalary(player, target, group, salary);
    }

    [Command("setrank", "Setze den Rang von einem Gruppenmitglied.", Permission.NONE,
        new[] { "Gruppen ID", "Spieler ID", "Level" })]
    public async void OnSetRank(ServerPlayer player, string expectedGroupId, string expectedPlayerId,
        string expectedLevel)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine gültige Gruppe Id an.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, group.Id, GroupPermission.MANAGE_MEMBERS))
        {
            player.SendNotification("Dein Charakter hat dafür nicht genügend Berechtigungen in der Gruppe.",
                NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe eine gültige Spieler ID an.", NotificationType.ERROR);
            return;
        }

        if (!uint.TryParse(expectedLevel, out var level))
        {
            player.SendNotification("Bitte gebe eine gültige Zahl als Gehalt an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, playerId);
        if (target == null)
        {
            return;
        }

        if (player == target)
        {
            player.SendNotification("Du kannst den Befehl nicht an dir selbst nutzen.", NotificationType.ERROR);
            return;
        }

        await _groupModule.SetMemberRank(player, group, target.CharacterModel.Id, level);
    }

    [Command("givegroup", "Übergebe eine Gruppe einen anderen Charakter.", Permission.NONE,
        new[] { "Gruppen ID", "Spieler ID" })]
    public async void OnGiveGroup(ServerPlayer player, string expectedGroupId, string expectedPlayerId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine gültige Gruppe Id an.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer von dieser Gruppe.",
                NotificationType.ERROR);
            return;
        }

        if (!int.TryParse(expectedPlayerId, out var playerId))
        {
            player.SendNotification("Bitte gebe eine gültige Spieler ID an.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, playerId);
        if (target == null)
        {
            return;
        }

        if (player == target)
        {
            player.SendNotification("Du kannst den Befehl nicht an dir selbst nutzen.", NotificationType.ERROR);
            return;
        }

        var data = new object[2];
        data[0] = group.Id;
        data[1] = target.Id;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.TWO_BUTTON_DIALOG,
            Title = "Gruppe übergeben",
            Description =
                $"Bist du sicher das du die Gruppe {group.Name} dem Charakter {target.CharacterModel.Name} vollständig überreichen möchtest?<br><br><span class='text-danger'>Dein Charakter ist dann nicht mehr Eigentümer dieser Gruppe!</span>",
            HasBankAccountSelection = false,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Übergeben",
            PrimaryButtonServerEvent = "group:givegroup",
            SecondaryButton = "Abbrechen"
        });
    }

    [Command("duty", "Gehe mit deinem Charakter in den Dienst.")]
    public async void OnDuty(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        if (groups == null || groups.Count == 0)
        {
            player.SendNotification("Dein Charakter ist in keiner Gruppe und kann nicht in den Dienst gehen.",
                NotificationType.ERROR);
            return;
        }

        if (await _houseService.GetByDistance(player.Position) is LeaseCompanyHouseModel leaseCompanyHouse)
        {
            var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY &&
                                                    g.Id == leaseCompanyHouse.GroupModelId);
            if (group == null)
            {
                player.SendNotification("Dein Charakter arbeitet hier nicht.", NotificationType.ERROR);
                return;
            }

            lock (player)
            {
                player.IsDuty = !player.IsDuty;
            }

            if (player.IsDuty)
            {
                await player.SetStreamSyncedMetaDataAsync("DUTY", (int)leaseCompanyHouse.LeaseCompanyType);
            }
            else
            {
                player.DeleteStreamSyncedMetaData("DUTY");
            }

            // We have to send an event here because we need the house id.
            player.EmitLocked("player:setduty", player.IsDuty, leaseCompanyHouse.Id);

            player.SendNotification(
                player.IsDuty ? "Du bist in den Dienst gegangen." : "Du bist aus dem Dienst gegangen.",
                NotificationType.SUCCESS);

            if (player.IsDuty)
            {
                leaseCompanyHouse.PlayerDuties++;
                player.DutyLeaseCompanyHouseId = leaseCompanyHouse.Id;

                _pedSyncModule.RemoveCashier(leaseCompanyHouse.Id);
            }
            else
            {
                leaseCompanyHouse.PlayerDuties--;

                if (leaseCompanyHouse.PlayerDuties >= 0)
                {
                    leaseCompanyHouse.PlayerDuties = 0;

                    if (leaseCompanyHouse.HasCashier)
                    {
                        _pedSyncModule.CreateCashier(leaseCompanyHouse);
                    }
                }
            }

            await _houseService.Update(leaseCompanyHouse);
            await _houseModule.UpdateOnClient(leaseCompanyHouse);

            return;
        }

        player.SendNotification("Du kannst hier nicht in den Dienst gehen.", NotificationType.ERROR);
    }

    [Command("togglecashier", "Aktiviere oder deaktiviere als Eigentümer des Unternehmens den NPC Kassierer.")]
    public async void OnToggleCashierState(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        if (groups == null || groups.Count == 0)
        {
            player.SendNotification("Dein Charakter ist in keiner Gruppe.", NotificationType.ERROR);
            return;
        }

        if (await _houseService.GetByDistance(player.Position) is LeaseCompanyHouseModel leaseCompanyHouse)
        {
            var group = groups?.FirstOrDefault(g => g.GroupType == GroupType.COMPANY &&
                                                    g.Id == leaseCompanyHouse.GroupModelId);
            if (group == null)
            {
                player.SendNotification("Dein Charakter ist nicht in dieser Gruppe.", NotificationType.ERROR);
                return;
            }

            leaseCompanyHouse.HasCashier = !leaseCompanyHouse.HasCashier;

            if (leaseCompanyHouse.HasCashier)
            {
                _pedSyncModule.CreateCashier(leaseCompanyHouse);
            }
            else
            {
                _pedSyncModule.RemoveCashier(leaseCompanyHouse.Id);
            }

            player.SendNotification(
                leaseCompanyHouse.HasCashier
                    ? "Du hast den Kassierer aktiviert, sollte kein Spieler im Dienst sein wird ein NPC die Kasse übernehmen."
                    : "Du hast den Kassierer deaktiviert, wenn kein Spieler im Dienst ist dann ist der Laden automatisch geschlossen.",
                NotificationType.SUCCESS);

            await _houseService.Update(leaseCompanyHouse);
            await _houseModule.UpdateOnClient(leaseCompanyHouse);
            return;
        }

        player.SendNotification("Du musst am pachtbaren Unternehmen stehen.", NotificationType.ERROR);
    }

    [Command("creategkey", "Erstelle einen neuen Gruppenschlüssel für deine Gruppe.", Permission.NONE,
        new[] { "Gruppen ID" })]
    public async void OnCreateGroupKey(ServerPlayer player, string expectedGroupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine gültige Gruppe Id an.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer von dieser Gruppe.",
                NotificationType.ERROR);
            return;
        }

        var catalogItem = await _itemCatalogService.GetByKey(ItemCatalogIds.GROUP_KEY);

        var data = new object[1];
        data[0] = group.Id;

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Gruppenschlüssel erstellen",
            Description =
                $"Möchtest du einen neuen Gruppenschlüssel für <b>${catalogItem.Price}</b> erstellen?<br><br><span class='text-muted'>Kosten werden von dem Gruppenkonto abgebucht.</span>",
            HasBankAccountSelection = false,
            FreezeGameControls = true,
            Data = data,
            PrimaryButton = "Erstellen",
            PrimaryButtonServerEvent = "group:creategroupkey"
        });
    }

    [Command("togglegveh", "Ändere das aktuelle Fahrzeug zu einem Gruppenfahrzeug oder Privatfahrzeug.",
        Permission.NONE, new[] { "Gruppen ID" })]
    public async void OnToggleGroupVehicle(ServerPlayer player, string expectedGroupId)
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

        if (!int.TryParse(expectedGroupId, out var groupId))
        {
            player.SendNotification("Bitte gebe eine gültige Gruppe Id an.", NotificationType.ERROR);
            return;
        }

        var group = await _groupService.GetByKey(groupId);
        if (group == null)
        {
            player.SendNotification("Es wurde keine Gruppe gefunden.", NotificationType.ERROR);
            return;
        }

        if (!_groupModule.IsOwner(player, group))
        {
            player.SendNotification("Dein Charakter ist nicht der Eigentümer von dieser Gruppe.",
                NotificationType.ERROR);
            return;
        }

        var vehicle = (ServerVehicle)player.Vehicle;

        if (vehicle.DbEntity.GroupModelOwnerId.HasValue)
        {
            if (vehicle.DbEntity.GroupModelOwnerId.Value != group.Id)
            {
                player.SendNotification("Das Fahrzeug gehört zu einer anderen Gruppe.", NotificationType.ERROR);
                return;
            }

            player.CreateDialog(new DialogData
            {
                Type = DialogType.ONE_BUTTON_DIALOG,
                Title = "Gruppenschlüssel erstellen",
                Description =
                    "Möchtest du dieses Fahrzeug von einem Gruppenfahrzeug zu deinem Privatfahrzeug machen?<br><br><span class='text-muted'>Gruppenschlüssel funktionieren dann bei diesem Fahrzeug nicht mehr.</span>",
                FreezeGameControls = true,
                PrimaryButton = "Wechseln",
                PrimaryButtonServerEvent = "group:switchtoprivatevehicle"
            });
        }
        else
        {
            if (!vehicle.DbEntity.CharacterModelId.HasValue ||
                player.CharacterModel.Id != vehicle.DbEntity.CharacterModelId.Value)
            {
                player.SendNotification("Dein Charakter ist nicht der Eigentümer von diesem Fahrzeug.",
                    NotificationType.ERROR);
                return;
            }

            var data = new object[1];
            data[0] = group.Id;

            player.CreateDialog(new DialogData
            {
                Type = DialogType.ONE_BUTTON_DIALOG,
                Title = "Gruppenschlüssel erstellen",
                Description =
                    "Möchtest du dieses Fahrzeug welches dein Privatfahrzeug ist zu einem Gruppenfahrzeug machen machen?<br><br><span class='text-muted'>Mitglieder in deiner Gruppe mit einem Gruppenschlüssel können dieses Fahrzeug dann verwenden.</span>",
                FreezeGameControls = true,
                Data = data,
                PrimaryButton = "Wechseln",
                PrimaryButtonServerEvent = "group:switchtogroupvehicle"
            });
        }
    }

    #endregion

    #region Delivery

    [Command("collect", "Lade eine Lieferung auf.")]
    public async void OnCollect(ServerPlayer player)
    {
        await _deliveryModule.CollectDelivery(player);
    }

    [Command("deploy", "Liefere eine Lieferung ab.", Permission.NONE, null, CommandArgs.NOT_GREEDY,
        new[] { "deliver" })]
    public async void OnDeploy(ServerPlayer player)
    {
        await _deliveryModule.DeployDelivery(player);
    }

    #endregion
}