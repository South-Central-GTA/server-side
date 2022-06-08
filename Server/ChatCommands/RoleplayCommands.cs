using System;
using System.Linq;
using AltV.Net.Async;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Modules.Chat;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server.ChatCommands;

internal class RoleplayCommands : ISingletonScript
{
    private readonly ChatModule _chatModule;

    private readonly Random _random = new();


    public RoleplayCommands(ChatModule chatModule)
    {
        _chatModule = chatModule;
    }

    [Command("kill", "Setze deine eigenen Leben auf null.")]
    public async void OnKill(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await player.SetHealthAsync(0);
    }

    [Command("do",
             "Definiere etwas in unserer Spielwelt.",
             Permission.NONE,
             new[] { "Definition" },
             CommandArgs.GREEDY)]
    public async void OnDo(ServerPlayer player, string message)
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

        await _chatModule.SendProxMessage(player, 30, ChatType.DO, message);
    }

    [Command("emote",
             "Definiere eine sichtbare Aktion deines Charakters.",
             Permission.NONE,
             new[] { "Tätigkeit" },
             CommandArgs.GREEDY,
             new[] { "me" })]
    public async void OnEmote(ServerPlayer player, string message)
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

        await _chatModule.SendProxMessage(player, 30, ChatType.EMOTE, message);
    }

    [Command("lowemote",
             "Definiere eine sichtbare Aktion deines Charakters.",
             Permission.NONE,
             new[] { "Tätigkeit" },
             CommandArgs.GREEDY,
             new[] { "lme" })]
    public async void OnLowEmote(ServerPlayer player, string message)
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

        await _chatModule.SendProxMessage(player, 5, ChatType.LOW_EMOTE, message);
    }

    [Command("my",
             "Emote etwas was mit deinem Charakter passiert oder mit seinem Eigentum.",
             Permission.NONE,
             new[] { "Definition" },
             CommandArgs.GREEDY)]
    public async void OnMy(ServerPlayer player, string message)
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

        await _chatModule.SendProxMessage(player, 15, ChatType.MY, message);
    }

    [Command("ooc",
             "Schreibe etwas außerhalb des Rollenspiels zu anderen Spielern.",
             Permission.NONE,
             new[] { "Nachricht" },
             CommandArgs.GREEDY,
             new[] { "b" })]
    public async void OnOutOfCharacter(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Du kannst jetzt nicht den OOC Chat benutzen.", NotificationType.ERROR);
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        await _chatModule.SendProxMessage(player, 25, ChatType.OOC, message);
    }

    [Command("scream",
             "Schreie/rufe eine Nachricht als dein Charakter.",
             Permission.NONE,
             new[] { "Nachricht" },
             CommandArgs.GREEDY,
             new[] { "s" })]
    public async void OnScream(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt nicht schreien.", NotificationType.ERROR);
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        await _chatModule.SendProxMessage(player, 40, ChatType.SCREAM, message);
    }

    [Command("whisper",
             "Flüstere eine Nachricht als dein Charakter.",
             Permission.NONE,
             new[] { "Nachricht" },
             CommandArgs.GREEDY,
             new[] { "w", "lr", "low", "fl" })]
    public async void OnWhisper(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt nicht flüstern.", NotificationType.ERROR);
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        await _chatModule.SendProxMessage(player, 3, ChatType.WISPER, message);
    }

    [Command("dice", "Rolle eine zufällige Zahl.", Permission.NONE, new[] { "Zahl" })]
    public async void OnDice(ServerPlayer player, string expectedNumber)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!int.TryParse(expectedNumber, out var highestNumber))
        {
            player.SendNotification("Gebe eine ganze Zahl an.", NotificationType.ERROR);
            return;
        }

        if (highestNumber < 0)
        {
            player.SendNotification("Gebe eine positive Zahl an.", NotificationType.ERROR);
            return;
        }

        var randomNumber = _random.Next(0, highestNumber);
        await _chatModule.SendProxMessage(player, 3, ChatType.EMOTE, $" würfelt {randomNumber}/{highestNumber}.");
    }

    [Command("megaphone",
             "Spreche als dein Charakter durch ein Megafon.",
             Permission.NONE,
             new[] { "Nachricht" },
             CommandArgs.GREEDY,
             new[] { "m" })]
    public async void OnMegafon(ServerPlayer player, string message)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            player.SendNotification("Dein Charakter kann jetzt kein Megafon benutzen.", NotificationType.ERROR);
            return;
        }

        message = message.TrimEnd().TrimStart();

        if (message.Length == 0)
        {
            player.SendNotification("Man kann keine leeren Nachrichten verschicken.", NotificationType.ERROR);
            return;
        }

        var item = player.CharacterModel.InventoryModel.Items
                         .FirstOrDefault(i => i.CatalogItemModelId == ItemCatalogIds.MEGAPHONE);
        if (item == null)
        {
            player.SendNotification("Dein Charakter hat kein Megafon im Inventar.", NotificationType.ERROR);
            return;
        }

        await _chatModule.SendProxMessage(player, 70, ChatType.MEGAPHONE, message);
    }
}