using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.DefinedJob;

namespace Server.Handlers.DefinedJob;

public class OpenDefinedJobMenuHandler : ISingletonScript
{
    private readonly DefinedJobModule _definedJobModule;

    public OpenDefinedJobMenuHandler(DefinedJobModule definedJobModule)
    {
        _definedJobModule = definedJobModule;

        AltAsync.OnClient<ServerPlayer>("definedjob:requestmenu", OnRequestMenu);
    }

    private async void OnRequestMenu(ServerPlayer player)
    {
        await _definedJobModule.OpenJobMenu(player);
    }
}