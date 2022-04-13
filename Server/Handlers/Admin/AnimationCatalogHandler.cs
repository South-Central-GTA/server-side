using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.Admin;

public class AnimationCatalogHandler : ISingletonScript
{
    private readonly AnimationService _animationService;
    
    public AnimationCatalogHandler(AnimationService animationService)
    {
        _animationService = animationService;
        
        AltAsync.OnClient<ServerPlayer>("animationcatalog:open", OnOpenAnimationCatalog);
        AltAsync.OnClient<ServerPlayer>("animationcatalog:useropen", OnUserOpenAnimationCatalog);
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
}