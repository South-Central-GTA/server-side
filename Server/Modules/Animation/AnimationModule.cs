using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Models;
using Server.Database.Enums;

namespace Server.Modules.Animation;

public class AnimationModule : ISingletonScript
{
    private readonly ILogger<AnimationModule> _logger;

    public AnimationModule(
        ILogger<AnimationModule> logger)
    {
        _logger = logger;
    }

    public void PlayAnimation(ServerPlayer player, string dict, string clip, AnimationOptions? options = null)
    {
        player.EmitLocked("animation:play", dict, clip, options);
    }

    public void ClearAnimation(ServerPlayer player)
    {
        if (player.CharacterModel.DeathState == DeathState.DEAD)
        {
            return;
        }
        
        player.EmitLocked("animation:clear");
    }
}