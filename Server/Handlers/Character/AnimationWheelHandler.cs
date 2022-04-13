using System.Collections.Generic;
using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Models;

namespace Server.Handlers.Character;

public class AnimationWheelHandler : ISingletonScript
{
    private readonly AnimationService _animationService;
    
    public AnimationWheelHandler(AnimationService animationService)
    {
        _animationService = animationService;
        
        AltAsync.OnClient<ServerPlayer>("animationswheel:requestmenu", OnRequestMenu);
    }

    private async void OnRequestMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var playerAnimations = new List<AnimationModel>();
        var toDelete = new List<int>();

        if (player.CharacterModel.AnimationIds.Count == 0)
        {
            player.SendNotification("Du hast keine Animationen in deinem Schnellmenu.", NotificationType.ERROR);
            return;
        }

        foreach (var animationId in player.CharacterModel.AnimationIds)
        {
            var animation = await _animationService.GetByKey(animationId);
            if (animation != null)
            {
                playerAnimations.Add(animation);
            }
            else
            {
                toDelete.Add(animationId);
            }
        }

        player.CharacterModel.AnimationIds = player.CharacterModel.AnimationIds.Except(toDelete).ToList();

        player.EmitLocked("animationwheel:showmenu", playerAnimations);
    }
}