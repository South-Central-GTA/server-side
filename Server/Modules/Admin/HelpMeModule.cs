using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;

namespace Server.Modules.Admin;

public class HelpMeModule : ISingletonScript
{
    private readonly AdminModule _adminModule;
    private readonly ILogger<HelpMeModule> _logger;
    private readonly List<HelpMeTicketData> _tickets = new();

    public HelpMeModule(
        ILogger<HelpMeModule> logger,
        AdminModule adminModule)
    {
        _logger = logger;

        _adminModule = adminModule;
    }

    public void CreateTicket(ServerPlayer player, string message)
    {
        if (HasTicket(player))
        {
            player.SendNotification("Du hast schon ein Ticket eröffnet.", NotificationType.ERROR);
            return;
        }

        _tickets.Add(new HelpMeTicketData { CreatorName = player.AccountName, CreatorDiscordId = player.DiscordId, Context = message });

        var staff = _adminModule.GetAllStaffPlayers();

        player.SendNotification(staff.Count == 0
                                    ? "Dein Ticket wurde erstellt, jedoch ist leider kein Team Mitglied online."
                                    : "Dein Ticket wurde erfolgreich erstellt.",
                                NotificationType.SUCCESS);

        foreach (var serverPlayer in staff)
        {
            serverPlayer.SendNotification("Ein neues HelpMe Ticket ist eingegangen.", NotificationType.INFO);
        }

        UpdateTicketsUi();
    }

    public void DeleteTicket(ServerPlayer player)
    {
        if (!HasTicket(player))
        {
            player.SendNotification("Du hast noch kein Ticket eröffnet.", NotificationType.ERROR);
            return;
        }

        _tickets.RemoveAll(t => t.CreatorDiscordId == player.DiscordId);
        player.SendNotification("Du hast dein Ticket gelöscht.", NotificationType.SUCCESS);

        UpdateTicketsUi();
    }

    public List<HelpMeTicketData> GetAllTickets()
    {
        return _tickets;
    }

    private bool HasTicket(ServerPlayer player)
    {
        return _tickets.FindIndex(t => t.CreatorDiscordId == player.DiscordId) != -1;
    }

    public void TakeTicket(ulong playerDiscordId)
    {
        var ticketData = _tickets.Find(t => t.CreatorDiscordId == playerDiscordId);
        _tickets.Remove(ticketData);
        UpdateTicketsUi();
    }

    private void UpdateTicketsUi()
    {
        var callback = new Action<ServerPlayer>(player =>
        {
            player.EmitGui("helpme:update", GetAllTickets());
        });

        _adminModule.GetAllStaffPlayers().ForEach(callback);
    }
}