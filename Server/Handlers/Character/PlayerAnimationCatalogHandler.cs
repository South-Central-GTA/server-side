using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Character;

public class PlayerAnimationCatalogHandler : ISingletonScript
{
    private readonly AnimationService _animationService;
    private readonly CharacterService _characterService;
    
    public PlayerAnimationCatalogHandler(
        AnimationService animationService, 
        CharacterService characterService)
    {
        _animationService = animationService;
        _characterService = characterService;

        AltAsync.OnClient<ServerPlayer, int>("animationcatalog:addplayeranimation", OnAddPlayerAnimation);
        AltAsync.OnClient<ServerPlayer, int>("animationcatalog:removeplayeranimation", OnRemovePlayerAnimation);
    }

    private async void OnOpenAnimationCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.AccountModel.Permission.HasFlag(Permission.MANAGE_ANIMATIONS))
        {
            player.SendNotification("Du hast nicht genug Berechtigungen.", NotificationType.ERROR);
            return;
        }

        player.EmitGui("animationcatalog:setup", await _animationService.GetAll());
    }

    private async void OnUserOpenAnimationCatalog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.EmitGui("animationcatalog:usersetup", await _animationService.GetAll(), player.CharacterModel.AnimationIds);
    }

    private async void OnAddPlayerAnimation(ServerPlayer player, int animationId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (player.CharacterModel.AnimationIds.Count >= player.AccountModel.MaxAnimations)
        {
            player.SendNotification($"Du kannst nicht mehr als {player.AccountModel.MaxAnimations} Animationen im Schnellmenu haben.", NotificationType.ERROR);
            return;
        }

        if (player.CharacterModel.AnimationIds.Contains(animationId))
        {
            return;
        }

        player.CharacterModel.AnimationIds.Add(animationId);
            
        await _characterService.Update(player.CharacterModel);
        player.EmitGui("animationcatalog:addplayeranim", animationId);
    }

    private async void OnRemovePlayerAnimation(ServerPlayer player, int animationId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!player.CharacterModel.AnimationIds.Contains(animationId))
        {
            return;
        }

        player.CharacterModel.AnimationIds.Remove(animationId);
    
        await _characterService.Update(player.CharacterModel);
        player.EmitGui("animationcatalog:removelayeranim", animationId);
    }
}