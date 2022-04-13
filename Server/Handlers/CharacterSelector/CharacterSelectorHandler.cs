using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Character;

namespace Server.Handlers.CharacterSelector;

public class CharacterSelectorHandler : ISingletonScript
{
    private readonly CharacterSelectionModule _characterSelectionModule;
    
    public CharacterSelectorHandler(CharacterSelectionModule characterSelectionModule)
    {
        _characterSelectionModule = characterSelectionModule;
        
        AltAsync.OnClient<ServerPlayer, int>("charselector:play", OnPlayCharacter);
    }

    private async void OnPlayCharacter(ServerPlayer player, int characterId)
    {
        if (!player.Exists)
        {
            return;
        }

        await _characterSelectionModule.SelectCharacter(player, characterId);
    }
}