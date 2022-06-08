using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class AdminMenuHandler : ISingletonScript
{
    public AdminMenuHandler()
    {
        AltAsync.OnClient<ServerPlayer>("admin:requestmenu", OnRequestMenu);
    }

    private async void OnRequestMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.STAFF))
        {
            return;
        }

        player.EmitLocked("admin:showmenu");
    }
}