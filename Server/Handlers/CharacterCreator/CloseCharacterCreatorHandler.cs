using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Modules.Character;

namespace Server.Handlers.CharacterCreator;

public class CloseCharacterCreatorHandler : ISingletonScript
{
    private readonly WorldLocationOptions _worldLocationOptions;
    private readonly CharacterCreationModule _characterCreationModule;
    private readonly CharacterSelectionModule _characterSelectionModule;

    public CloseCharacterCreatorHandler(
        IOptions<WorldLocationOptions> worldLocationOptions,
        CharacterCreationModule characterCreationModule,
        CharacterSelectionModule characterSelectionModule)
    {
        _worldLocationOptions = worldLocationOptions.Value;
        _characterCreationModule = characterCreationModule;
        _characterSelectionModule = characterSelectionModule;

        AltAsync.OnClient<ServerPlayer>("charcreator:close", OnClose);
        AltAsync.OnClient<ServerPlayer>("charcreator:resetcamera", OnResetCamera);
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

    private async void OnResetCamera(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.SetPositionLocked(new Position(_worldLocationOptions.CharacterSelectionPositionX,
                                              _worldLocationOptions.CharacterSelectionPositionY,
                                              _worldLocationOptions.CharacterSelectionPositionZ));
        player.EmitLocked("charcreator:resetcamera");
    }
}