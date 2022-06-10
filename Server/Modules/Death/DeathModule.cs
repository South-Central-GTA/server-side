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
    private readonly AnimationModule _animationModule;
    private readonly CharacterService _characterService;
    private readonly string[] _deathClips = { "death_a", "death_b", "death_c" };

    private readonly DeathOptions _deathOptions;
    private readonly PhoneCallModule _phoneCallModule;
    private readonly Random _random = new();
    private readonly ReviveModule _reviveModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public DeathModule(IOptions<DeathOptions> deathOptions, IOptions<WorldLocationOptions> worldLocationOptions,
        PhoneCallModule phoneCallModule, ReviveModule reviveModule, CharacterService characterService,
        AnimationModule animationModule)
    {
        _phoneCallModule = phoneCallModule;
        _reviveModule = reviveModule;
        _characterService = characterService;
        _animationModule = animationModule;
        _deathOptions = deathOptions.Value;
        _worldLocationOptions = worldLocationOptions.Value;
    }

    public async Task PreparePlayerDead(ServerPlayer player, IEntity killer, uint weapon)
    {
        if (!player.Exists)
        {
            return;
        }

        player.CharacterModel.DeathState = DeathState.DEAD;
        player.SetSyncedMetaData("DEATH_STATE", player.CharacterModel.DeathState);


        await _characterService.Update(player);

        await player.SpawnAsync(player.Position);

        await SetPlayerDead(player);
    }

    public async Task SetPlayerDead(ServerPlayer player)
    {
        // Cancel all actions like calling
        _phoneCallModule.Hangup(player, true);

        _animationModule.PlayAnimation(player, "combat@death@from_writhe",
            _deathClips[_random.Next(_deathClips.Length)],
            new AnimationOptions { Flag = AnimationFlag.STOP_ON_LAST_FRAME, LockX = true, LockY = true });

        player.CreateTimer("player_respawn", (sender, args) => OnPlayerRespawnTimerCallback(player),
            1000 * 60 * _deathOptions.MinutesBeforeRespawn);

        player.EmitLocked("death:start");
        player.Invincible = true;
    }

    private async void OnPlayerRespawnTimerCallback(ServerPlayer player)
    {
        var position = new Position(_worldLocationOptions.RespawnPositionX, _worldLocationOptions.RespawnPositionY,
            _worldLocationOptions.RespawnPositionZ);
        await _reviveModule.AutoRevivePlayer(player, position);
    }
}