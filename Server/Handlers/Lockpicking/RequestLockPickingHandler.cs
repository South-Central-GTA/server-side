using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules;
using Server.Modules.Key;

namespace Server.Handlers;

public class RequestLockPickingHandler : ISingletonScript
{
    private readonly LockModule _lockModule;
    private readonly LockpickModule _lockpickModule;

    public RequestLockPickingHandler(LockpickModule lockpickModule, LockModule lockModule)
    {
        _lockpickModule = lockpickModule;
        _lockModule = lockModule;
        AltAsync.OnClient<ServerPlayer, int>("lockpicking:requestpick", OnExecute);
    }

    private async void OnExecute(ServerPlayer player, int dbId)
    {
        if (!player.Exists)
        {
            return;
        }

        var entities = await _lockModule.GetAllLockableEntities(player);
        var lockableEntity = entities.FirstOrDefault(e => e.Id == dbId);

        if (lockableEntity == null)
        {
            return;
        }
        
        await _lockpickModule.PickLockAsync(player, lockableEntity);
    }
}