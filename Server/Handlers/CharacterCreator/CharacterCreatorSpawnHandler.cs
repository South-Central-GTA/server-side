using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Character;

namespace Server.Handlers.CharacterCreator;

public class CharacterCreatorSpawnHandler : ISingletonScript
{
    private readonly CharacterSpawnModule _characterSpawnModule;

    public CharacterCreatorSpawnHandler(CharacterSpawnModule characterSpawnModule)
    {
        _characterSpawnModule = characterSpawnModule;

        AltAsync.OnClient<ServerPlayer>("charcreatorspawn:open", OnOpenSpawnSelectionMenu);
    }

    private async void OnOpenSpawnSelectionMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.EmitLocked("spawnselector:open", _characterSpawnModule.GetSpawns());
    }
}