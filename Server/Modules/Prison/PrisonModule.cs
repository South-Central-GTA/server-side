using System.Text.Json;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;

namespace Server.Modules.Prison;

public class PrisonModule : ITransientScript
{
    private readonly CharacterService _characterService;
    private readonly WorldLocationOptions _worldLocationOptions;

    public PrisonModule(IOptions<WorldLocationOptions> worldLocationOptions, CharacterService characterService)
    {
        _worldLocationOptions = worldLocationOptions.Value;

        _characterService = characterService;
    }

    public void SetPlayerInPrison(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.SetUniqueDimension();
        player.SetPositionLocked(new Position(_worldLocationOptions.FreeJailPositionX,
            _worldLocationOptions.FreeJailPositionY, _worldLocationOptions.FreeJailPositionZ));

        player.EmitLocked("prison:start", JsonSerializer.Serialize(player.CharacterModel.JailedUntil));
    }

    public async Task ClearPlayerFromPrison(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        await player.SetDimensionAsync(0);
        player.SetPositionLocked(new Position(_worldLocationOptions.FreeJailPositionX,
            _worldLocationOptions.FreeJailPositionY, _worldLocationOptions.FreeJailPositionZ));

        player.CharacterModel.JailedUntil = null;
        await _characterService.Update(player.CharacterModel);

        player.EmitLocked("prison:stop");
    }
}