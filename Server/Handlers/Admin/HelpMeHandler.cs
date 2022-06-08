using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Database.Enums;
using Server.Modules.Admin;

namespace Server.Handlers.Admin;

public class HelpMeHandler : ISingletonScript
{
    private readonly HelpMeModule _helpMeModule;

    public HelpMeHandler(HelpMeModule helpMeModule)
    {
        _helpMeModule = helpMeModule;

        AltAsync.OnClient<ServerPlayer>("helpme:open", OnOpenHelpMes);
        AltAsync.OnClient<ServerPlayer, string>("helpme:taketicket", OnTakeTicket);
    }

    private void OnOpenHelpMes(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitGui("helpme:setup", _helpMeModule.GetAllTickets());
    }

    private void OnTakeTicket(ServerPlayer player, string playerDiscordId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        var discordId = ulong.Parse(playerDiscordId);
        var ticketCreator = Alt.GetAllPlayers().FindPlayerByDiscordId(discordId);
        _helpMeModule.TakeTicket(discordId);

        ticketCreator.SendNotification("Dein Ticket wurde von Team Mitglied " + player.AccountName + " angenommen.",
                                       NotificationType.INFO);
        player.SendNotification("Du hast das Ticket von " + ticketCreator.AccountName + " (ID: " + ticketCreator.Id +
                                ") angenommen, teleportiere dich bitte zum Spieler.",
                                NotificationType.INFO);
    }
}