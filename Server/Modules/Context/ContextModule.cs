using System.Collections.Generic;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Models;

namespace Server.Modules.Context;

public class ContextModule : ISingletonScript
{
    private readonly ILogger<ContextModule> _logger;

    public ContextModule(
        ILogger<ContextModule> logger)
    {
        _logger = logger;
    }

    public void OpenMenu(ServerPlayer player, string title, List<ActionData> actions)
    {
        player.EmitLocked("contextmenu:open", title, actions);
    }
}