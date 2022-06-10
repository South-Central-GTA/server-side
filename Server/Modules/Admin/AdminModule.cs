using System.Collections.Generic;
using AltV.Net;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Database.Enums;

namespace Server.Modules.Admin;

public class AdminModule : ITransientScript
{
    private readonly ILogger<AdminModule> _logger;

    public AdminModule(ILogger<AdminModule> logger)
    {
        _logger = logger;
    }

    public List<ServerPlayer> GetAllStaffPlayers()
    {
        return Alt.GetAllPlayers()
            .Where(p => p.Exists && p.IsSpawned && p.AccountModel.Permission.HasFlag(Permission.STAFF));
    }
}