using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class DeleteDirectoryHandler : ISingletonScript
{
    private readonly DirectoryService _directoryService;
    private readonly FileService _fileService;

    private readonly GroupModule _groupModule;
    private readonly SyncFileModule _syncFileModule;
    
    public DeleteDirectoryHandler(
        DirectoryService directoryService,
        FileService fileService,
        
        GroupModule groupModule, 
        SyncFileModule syncFileModule)
    {
        _directoryService = directoryService;
        _fileService = fileService;
        
        _groupModule = groupModule;
        _syncFileModule = syncFileModule;

        AltAsync.OnClient<ServerPlayer, int>("filesystem:deletedirectory", OnRequestDirectoryName);
    }

    private async void OnRequestDirectoryName(ServerPlayer player, int directoryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var directory = await _directoryService.GetByKey(directoryId);
        if (directory == null)
        {
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id, directory.GroupModelId, GroupPermission.MDC_OPERATOR))
        {
            return;
        }

        var files = await _fileService.Where(f => f.DirectoryModelId == directoryId);
        await _fileService.RemoveRange(files);
        
        await _directoryService.Remove(directory);
        await _syncFileModule.UpdateDirectory(directory.GroupModelId);
    }
}