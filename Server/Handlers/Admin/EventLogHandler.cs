using System;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class EventLogHandler : ISingletonScript
{
    private readonly CommandLogService _commandLogService;

    public EventLogHandler(CommandLogService commandLogService)
    {
        _commandLogService = commandLogService;
        
        AltAsync.OnClient<ServerPlayer>("eventlog:open", OnOpenItemCatalog);
    }

    private async void OnOpenItemCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.ADMIN))
        {
            player.SendNotification("Du hast nicht genug Berechtigungen.", NotificationType.ERROR);
            return;
        }

        var teamAccountIds = Alt.GetAllPlayers()
                                .Where(p => p.AccountModel.Permission.HasFlag(Permission.STAFF))
                                .Select(p => p.AccountModel.SocialClubId);

        var less30days = DateTime.Now.AddDays(-30);

        var commands = await _commandLogService
            .Where(c => teamAccountIds.Contains(c.AccountModelId));

        commands = commands.Where(c => c.RequiredPermission.HasFlag(Permission.STAFF)).ToList();

        player.EmitGui("eventlog:setup", commands.Where(c => c.LoggedAt > less30days).ToList());
    }
}