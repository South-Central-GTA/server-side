using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Group;

namespace Server.Handlers.Group;

public class RequestMenuGroupHandler : ISingletonScript
{
    private readonly GroupModule _groupModule;

    public RequestMenuGroupHandler(GroupModule groupModule)
    {
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer>("groupmenu:requestmenu", OnRequestMenu);
    }

    private async void OnRequestMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await _groupModule.UpdateUi(player);
    }
}