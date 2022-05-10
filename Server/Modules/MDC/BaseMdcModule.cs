using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Mdc;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Modules.MDC;

public class BaseMdcModule
    : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly BulletInService _bulletInService;
    private readonly GroupModule _groupModule;

    public BaseMdcModule(
        GroupFactionService groupFactionService,
        BulletInService bulletInService,
        GroupModule groupModule)
    {
        _groupFactionService = groupFactionService;
        _bulletInService = bulletInService;
        _groupModule = groupModule;
    }

    public async Task UpdateOperatorPermissionUi(ServerPlayer player)
    {
        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup == null)
        {
            return;
        }
        
        var isOperator =
           await _groupModule.HasPermission(player.CharacterModel.Id, factionGroup.Id, GroupPermission.MDC_OPERATOR);
        
        player.EmitGui("mdc:updateoperatorpermission", isOperator); 
    }

    public async Task<List<BulletInEntryModel>> GetBulletInEntries(FactionType factionType)
    {
        return await _bulletInService.Where(b => b.FactionType == factionType);
    }
}