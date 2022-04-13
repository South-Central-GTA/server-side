using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.Character;

namespace Server.Handlers.CharacterCreator;

public class CloseCharacterCreatorHandler : ISingletonScript
{
    private readonly CharacterCreationModule _characterCreationModule;
    private readonly CharacterSelectionModule _characterSelectionModule;
    
    public CloseCharacterCreatorHandler(CharacterCreationModule characterCreationModule, 
                                        CharacterSelectionModule characterSelectionModule)
    {
        _characterCreationModule = characterCreationModule;
        _characterSelectionModule = characterSelectionModule;
        
        AltAsync.OnClient<ServerPlayer>("charcreator:close", OnClose);
    }

    private async void OnClose(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await _characterCreationModule.CloseAsync(player);
        await _characterSelectionModule.OpenAsync(player);
    }
}