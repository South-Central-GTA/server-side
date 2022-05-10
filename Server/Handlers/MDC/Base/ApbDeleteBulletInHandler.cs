using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;

namespace Server.Handlers.MDC.Base;

public class ApbDeleteBulletInHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly BulletInService _bulletInService;
 
    public ApbDeleteBulletInHandler(
        GroupFactionService groupFactionService, 
        BulletInService bulletInService) 
    {
        _groupFactionService = groupFactionService;
        _bulletInService = bulletInService;

        AltAsync.OnClient<ServerPlayer, FactionType, int>("apb:deletebulletin", OnExecute);
    }

    private async void OnExecute(ServerPlayer player, FactionType factionType, int entryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup == null || factionGroup.FactionType != factionType)
        {
            return;
        }

        var entry = await _bulletInService.GetByKey(entryId);
        if (entry == null)
        {
            return;
        }

        await _bulletInService.Remove(entry);
        
        // TODO: Update for each player that has the apb menu open.
    }
}