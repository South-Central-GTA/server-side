using System;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Animation;
using Server.Modules.Phone;

namespace Server.Modules.Death;

public class DeathModule : ISingletonScript
{
    private readonly string[] _deathClips = { "death_a", "death_b", "death_c" };
    private readonly Random _random = new();
    
    private readonly DeathOptions _deathOptions;
    private readonly WorldLocationOptions _worldLocationOptions;
    private readonly AnimationModule _animationModule;
    private readonly PhoneCallModule _phoneCallModule;
    private readonly ReviveModule _reviveModule;
    private readonly CharacterService _characterService;

    public DeathModule(IOptions<DeathOptions> deathOptions,
                       IOptions<WorldLocationOptions> worldLocationOptions, 
                       AnimationModule animationModule, 
                       PhoneCallModule phoneCallModule, 
                       ReviveModule reviveModule, 
                       CharacterService characterService)
    {
        _animationModule = animationModule;
        _phoneCallModule = phoneCallModule;
        _reviveModule = reviveModule;
        _characterService = characterService;
        _deathOptions = deathOptions.Value;
        _worldLocationOptions = worldLocationOptions.Value;
    }

    public async Task PreparePlayerDead(ServerPlayer player, IEntity killer, uint weapon)
    {
        if (!player.Exists || player.CharacterModel.DeathState == DeathState.DEAD)
        {
            return;
        }

        player.CharacterModel.DeathState = DeathState.DEAD;
        player.SetSyncedMetaData("DEATH_STATE", player.CharacterModel.DeathState);
        await _characterService.Update(player.CharacterModel);

        await player.SpawnAsync(player.Position, 0);

        SetPlayerDead(player);
    }

    public void SetPlayerDead(ServerPlayer player)
    {
        // Cancel all actions like calling
        _phoneCallModule.Hangup(player, true);
        
        _animationModule.PlayAnimation(player, "combat@death@from_writhe", _deathClips[_random.Next(_deathClips.Length)], new AnimationOptions
        {
            Flag = AnimationFlag.STOP_ON_LAST_FRAME
        });

        player.CreateTimer("player_respawn", (sender, args) => OnPlayerRespawnTimerCallback(player), 1000 * 60 * _deathOptions.MinutesBeforeRespawn);
    }

    private async void OnPlayerRespawnTimerCallback(ServerPlayer player)
    {
        var position = new Position(_worldLocationOptions.RespawnPositionX, _worldLocationOptions.RespawnPositionY, _worldLocationOptions.RespawnPositionZ);
        await _reviveModule.AutoRevivePlayer(player, position);
    }
}