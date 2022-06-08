using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.MDC.Base;

public class ApbCreateBulletInHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly BulletInService _bulletInService;
    private readonly ApbModule _apbModule;
    private readonly GroupModule _groupModule;

    public ApbCreateBulletInHandler(
        GroupFactionService groupFactionService,
        BulletInService bulletInService,
        ApbModule apbModule,
        GroupModule groupModule)
    {
        _groupFactionService = groupFactionService;
        _bulletInService = bulletInService;
        _apbModule = apbModule;
        _groupModule = groupModule;

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

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, factionGroup.Id, GroupPermission.MDC_OPERATOR))
        {
            return;
        }

        await _bulletInService.Add(new BulletInEntryModel()
        {
            CreatorCharacterName = player.CharacterModel.Name,
            Content = input,
            FactionType = factionType
        });

        await _apbModule.UpdateUi(factionGroup.Id);
    }
}