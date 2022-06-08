using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Character;

namespace Server.Handlers.Prison;

public class PrisonOpenCharacterSelectionHandler : ISingletonScript
{
    private readonly CharacterSelectionModule _characterSelectionModule;

    public PrisonOpenCharacterSelectionHandler(CharacterSelectionModule characterSelectionModule)
    {
        _characterSelectionModule = characterSelectionModule;

        AltAsync.OnClient<ServerPlayer>("prison:requestcharacterselection", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.CharacterModel.JailedUntil.HasValue)
        {
            return;
        }

        await _characterSelectionModule.OpenAsync(player);
    }
}