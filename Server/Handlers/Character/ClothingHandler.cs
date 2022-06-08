using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;

namespace Server.Handlers.Character;

public class ClothingHandler : ISingletonScript
{
    private readonly CharacterService _characterService;

    public ClothingHandler(CharacterService characterService)
    {
        _characterService = characterService;

        AltAsync.OnClient<ServerPlayer, int, int>("settorsomenu:savetorso", OnSaveTorso);
    }

    private async void OnSaveTorso(ServerPlayer player, int drawableId, int textureId)
    {
        if (!player.Exists)
        {
            return;
        }

        player.CharacterModel.Torso = drawableId;
        player.CharacterModel.TorsoTexture = textureId;

        await _characterService.Update(player.CharacterModel);
    }
}