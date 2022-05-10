using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;

namespace Server.Handlers.MDC.Base;

public class ApbCreateBulletInHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly BulletInService _bulletInService;
 
    public ApbCreateBulletInHandler(
        GroupFactionService groupFactionService, 
        BulletInService bulletInService) 
    {
        _groupFactionService = groupFactionService;
        _bulletInService = bulletInService;

        AltAsync.OnClient<ServerPlayer, FactionType, string>("apb:createbulletin", OnExecute);
    }

    private async void OnExecute(ServerPlayer player, FactionType factionType, string input)
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
        
        await _bulletInService.Add(new BulletInEntryModel()
        {
            CreatorCharacterName = player.CharacterModel.Name, 
            Content = input,
            FactionType = factionType
        });
        
        // TODO: Update for each player that has the apb menu open.
    }
}