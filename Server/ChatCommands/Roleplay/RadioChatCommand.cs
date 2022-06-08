using System.Linq;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Database.Models.Inventory;
using Server.Modules.Chat;
using Server.Modules.Group;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server.ChatCommands.Roleplay;

internal class RadioChatCommand : ISingletonScript
{
    private readonly ChatModule _chatModule;

    private readonly GroupModule _groupModule;
    private readonly RadioModule _radioModule;

    public RadioChatCommand(
        GroupModule groupModule,
        ChatModule chatModule,
        RadioModule radioModule)
    {
        _groupModule = groupModule;
        _chatModule = chatModule;
        _radioModule = radioModule;
    }

    [Command("r", "Spreche in dein Funkgerät.", Permission.NONE, new[] { "Funkspruch" }, CommandArgs.GREEDY)]
    public async void OnTalkRadio(ServerPlayer player, string message)
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

        if (player.HasData("RADIO_FREQUENCY"))
        {
            player.DeleteData("RADIO_FREQUENCY");
        }

        player.SetData("RADIO_FREQUENCY", radioItem.Frequency);

        await _radioModule.SendMessageOnFrequency(player, ChatType.RADIO_SPEAK, radioItem, message);

        await _chatModule.SendProxMessage(player, 15, ChatType.RADIO_SPEAK, message);
    }

    [Command("rs", "Schreie in dein Funkgerät.", Permission.NONE, new[] { "Funkspruch" }, CommandArgs.GREEDY)]
    public async void OnScreamRadio(ServerPlayer player, string message)
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

        if (player.HasData("RADIO_FREQUENCY"))
        {
            player.DeleteData("RADIO_FREQUENCY");
        }

        player.SetData("RADIO_FREQUENCY", radioItem.Frequency);

        await _radioModule.SendMessageOnFrequency(player, ChatType.RADIO_SCREAM, radioItem, message);

        await _chatModule.SendProxMessage(player, 40, ChatType.RADIO_SCREAM, message);
    }

    [Command("rlow", "Flüstere in dein Funkgerät.", Permission.NONE, new[] { "Funkspruch" }, CommandArgs.GREEDY)]
    public async void OnLowRadio(ServerPlayer player, string message)
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

        if (player.HasData("RADIO_FREQUENCY"))
        {
            player.DeleteData("RADIO_FREQUENCY");
        }

        player.SetData("RADIO_FREQUENCY", radioItem.Frequency);

        await _radioModule.SendMessageOnFrequency(player, ChatType.RADIO_WISPER, radioItem, message);

        await _chatModule.SendProxMessage(player, 3, ChatType.RADIO_WISPER, message);
    }
}