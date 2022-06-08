using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using ISingletonScript = Server.Core.Abstractions.ScriptStrategy.ISingletonScript;

namespace Server.Core.CommandSystem;

public class CommandEvents : ISingletonScript
{
    [CommandEvent(CommandEventType.NOT_FOUND)]
    public void OnCommandNotFound(ServerPlayer player, string commandName)
    {
        player.SendNotification("Ungültiger Befehl.", NotificationType.ERROR);
    }

    [CommandEvent(CommandEventType.MISSING_PERMISSION)]
    public void OnCommandMissingPermission(ServerPlayer player, string commandName)
    {
        player.SendNotification("Fehlende Berechtigung.", NotificationType.WARNING);
    }

    [CommandEvent(CommandEventType.ADUTY_REQUIRED)]
    public void OnCommandAdutyRequired(ServerPlayer player, string commandName)
    {
        player.SendNotification("Du musst im Admindienst sein.", NotificationType.ERROR);
    }

    [CommandEvent(CommandEventType.MISSING_ARGS)]
    public void OnCommandMissingArgs(ServerPlayer player, string commandName)
    {
        player.SendNotification("Du hast zu wenig Parameter angegeben.", NotificationType.ERROR);
    }
}