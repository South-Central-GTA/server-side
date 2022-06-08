using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class DeleteFileHandler : ISingletonScript
{
    private readonly FileService _fileService;

    private readonly GroupModule _groupModule;
    private readonly SyncFileModule _syncFileModule;

    public DeleteFileHandler(
        FileService fileService,
        GroupModule groupModule,
        SyncFileModule syncFileModule)
    {
        _fileService = fileService;

        _groupModule = groupModule;
        _syncFileModule = syncFileModule;

        AltAsync.OnClient<ServerPlayer, int>("filesystem:deletefile", OnRequestDirectoryName);
    }

    private async void OnRequestDirectoryName(ServerPlayer player, int directoryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var file = await _fileService.GetByKey(directoryId);
        if (file == null)
        {
            return;
        }

        if (!await _groupModule.HasPermission(player.CharacterModel.Id,
                                              file.DirectoryModel.GroupModelId,
                                              GroupPermission.MDC_OPERATOR))
        {
            return;
        }

        await _fileService.Remove(file);
        await _syncFileModule.UpdateDirectory(file.DirectoryModel.GroupModelId, file.DirectoryModelId);
    }
}