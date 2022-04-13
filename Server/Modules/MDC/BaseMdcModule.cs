using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Modules.MDC;

public class BaseMdcModule
    : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;
    private readonly GroupModule _groupModule;
    private readonly FileModule _fileModule;
    
    public BaseMdcModule(
        GroupFactionService groupFactionService, 
        GroupModule groupModule, 
        FileModule fileModule)
    {
        _groupFactionService = groupFactionService;
        _groupModule = groupModule;
        _fileModule = fileModule;
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
}