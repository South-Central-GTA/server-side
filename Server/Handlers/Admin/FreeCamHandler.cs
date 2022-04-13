using AltV.Net.Async;
using AltV.Net.Data;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Character;

namespace Server.Handlers.Admin;

public class FreeCamHandler : ISingletonScript
{
    private readonly CharacterSpawnModule _characterSpawnModule;

    public FreeCamHandler(CharacterSpawnModule characterSpawnModule)
    {
        _characterSpawnModule = characterSpawnModule;
        
        AltAsync.OnClient<ServerPlayer, float, float, float>("freecam:stop", OnFreeCamStop);
        AltAsync.OnClient<ServerPlayer, float, float, float>("freecam:update", OnUpdate);
    }

    private async void OnFreeCamStop(ServerPlayer player, float x, float y, float z)
    {
        if (!player.Exists)
        {
            return;
        }

        await _characterSpawnModule.Spawn(player, new Position(x, y, z), new Rotation(0, 0, 0), 0);
    }

    private async void OnUpdate(ServerPlayer player, float x, float y, float z)
    {
        if (!player.Exists)
        {
            return;
        }

        player.SetPositionLocked(new Position(x, y, z));
    }
}