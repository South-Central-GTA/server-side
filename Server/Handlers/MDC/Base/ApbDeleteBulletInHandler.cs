using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.MDC.Base;

public class ApbDeleteBulletInHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly BulletInService _bulletInService;
    private readonly ApbModule _apbModule;
    private readonly GroupModule _groupModule;

    public ApbDeleteBulletInHandler(
        GroupFactionService groupFactionService,
        BulletInService bulletInService,
        ApbModule apbModule,
        GroupModule groupModule)
    {
        _groupFactionService = groupFactionService;
        _bulletInService = bulletInService;
        _apbModule = apbModule;
        _groupModule = groupModule;

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

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, factionGroup.Id, GroupPermission.MDC_OPERATOR))
        {
            return;
        }

        var entry = await _bulletInService.GetByKey(entryId);
        if (entry == null)
        {
            return;
        }

        await _bulletInService.Remove(entry);

        await _apbModule.UpdateUi(factionGroup.Id);
    }
}