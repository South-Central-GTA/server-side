using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Models.File;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class SetFileReadPermissionHandler : ISingletonScript
{
    private readonly DirectoryService _directoryService;

    private readonly GroupModule _groupModule;
    
    public SetFileReadPermissionHandler(
        DirectoryService directoryService,
        
        GroupModule groupModule)
    {
        _directoryService = directoryService;
        
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int>("filesystem:setreadpermission", OnExecuteEvent);
    }

    private async void OnExecuteEvent(ServerPlayer player, int fileId, int groupLevel)
    {
        if (!player.Exists)
        {
            return;
        }

        var directory = await _directoryService.GetByKey(fileId);
        if (directory == null)
        {
            return;
        }

        if (!_groupModule.IsOwner(player, directory.GroupModel))
        {
            return;
        }

        directory.ReadGroupLevel = groupLevel;
        
        await _directoryService.Update(directory);
    }
}