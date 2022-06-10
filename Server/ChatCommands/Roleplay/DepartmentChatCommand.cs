using System.Linq;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Chat;
using Server.Modules.Group;

namespace Server.ChatCommands.Roleplay;

internal class DepartmentChatCommand : ISingletonScript
{
    private readonly ChatModule _chatModule;

    private readonly GroupModule _groupModule;
    private readonly RadioModule _radioModule;

    public DepartmentChatCommand(GroupModule groupModule, ChatModule chatModule, RadioModule radioModule)
    {
        _groupModule = groupModule;
        _chatModule = chatModule;
        _radioModule = radioModule;
    }

    [Command("dep", "Spreche in den öffentlichen Fraktionsfunk.", Permission.NONE, new[] { "Funkspruch" },
        CommandArgs.GREEDY)]
    public async void OnDepartmentRadio(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt kein Funkgerät benutzen.", NotificationType.ERROR);
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        var radioItem = (ItemRadioModel)player.CharacterModel.InventoryModel.Items.OrderBy(i => i.Slot)
            .FirstOrDefault(i => i.CatalogItemModelId == ItemCatalogIds.RADIO);
        if (radioItem == null)
        {
            player.SendNotification("Dein Charakter hat kein Funkgerät im Inventar.", NotificationType.ERROR);
            return;
        }

        if (radioItem.FactionType == FactionType.CITIZEN)
        {
            player.SendNotification("Das Funkgerät deines Charakters kann nicht auf dieser Frequenz funken.",
                NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.FACTION))
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        await _radioModule.SendMessageOnDepartment(player, ChatType.DEP_SPEAK, radioItem, message);

        await _chatModule.SendProxMessage(player, 3, ChatType.DEP_SPEAK, message);
    }

    [Command("deplow", "Flüstere in den öffentlichen Fraktionsfunk.", Permission.NONE, new[] { "Funkspruch" },
        CommandArgs.GREEDY, new[] { "depl" })]
    public async void OnLowDepartmentRadio(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt kein Funkgerät benutzen.", NotificationType.ERROR);
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        var radioItem = (ItemRadioModel)player.CharacterModel.InventoryModel.Items.OrderBy(i => i.Slot)
            .FirstOrDefault(i => i.CatalogItemModelId == ItemCatalogIds.RADIO);
        if (radioItem == null)
        {
            player.SendNotification("Dein Charakter hat kein Funkgerät im Inventar.", NotificationType.ERROR);
            return;
        }

        if (radioItem.FactionType == FactionType.CITIZEN)
        {
            player.SendNotification("Das Funkgerät deines Charakters kann nicht auf dieser Frequenz funken.",
                NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.FACTION))
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        await _radioModule.SendMessageOnDepartment(player, ChatType.DEP_WISPER, radioItem, message);

        await _chatModule.SendProxMessage(player, 3, ChatType.DEP_WISPER, message);
    }

    [Command("depscream", "Schreie in den öffentlichen Fraktionsfunk.", Permission.NONE, new[] { "Funkspruch" },
        CommandArgs.GREEDY, new[] { "deps" })]
    public async void OnScreamDepartmentRadio(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt kein Funkgerät benutzen.", NotificationType.ERROR);
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        var radioItem = (ItemRadioModel)player.CharacterModel.InventoryModel.Items.OrderBy(i => i.Slot)
            .FirstOrDefault(i => i.CatalogItemModelId == ItemCatalogIds.RADIO);
        if (radioItem == null)
        {
            player.SendNotification("Dein Charakter hat kein Funkgerät im Inventar.", NotificationType.ERROR);
            return;
        }

        if (radioItem.FactionType == FactionType.CITIZEN)
        {
            player.SendNotification("Das Funkgerät deines Charakters kann nicht auf dieser Frequenz funken.",
                NotificationType.ERROR);
            return;
        }

        if (await _groupModule.IsPlayerInGroupType(player, GroupType.FACTION))
        {
            player.SendNotification("Dies würde dein Charakter nicht tun.", NotificationType.ERROR);
            return;
        }

        await _radioModule.SendMessageOnDepartment(player, ChatType.DEP_SCREAM, radioItem, message);

        await _chatModule.SendProxMessage(player, 3, ChatType.DEP_SCREAM, message);
    }
}