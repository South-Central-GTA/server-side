using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.DefinedJob;

namespace Server.Handlers.DefinedJob;

public class QuitDefinedJobHandler : ISingletonScript
{
    private readonly DefinedJobModule _definedJobModule;

    public QuitDefinedJobHandler(DefinedJobModule definedJobModule)
    {
        _definedJobModule = definedJobModule;

        AltAsync.OnClient<ServerPlayer>("definedjob:quit", OnQuitJob);
    }

    private async void OnQuitJob(ServerPlayer player)
    {
        await _definedJobModule.QuitJob(player);
    }
}