using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class RevokeEditFileHandler : ISingletonScript
{
    private readonly FileService _fileService;

    private readonly GroupModule _groupModule;

    public RevokeEditFileHandler(FileService fileService, GroupModule groupModule)
    {
        _fileService = fileService;

        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("filesystem:revokeeditfile", OnRevokeEditFile);
    }

    private async void OnRevokeEditFile(ServerPlayer player, int fileId)
    {
        if (!player.Exists)
        {
            return;
        }

        var file = await _fileService.GetByKey(fileId);
        if (file == null)
        {
            return;
        }

        if (!await _groupModule.IsPlayerInGroup(player, file.DirectoryModel.GroupModelId))
        {
            return;
        }

        file.IsBlocked = false;

        await _fileService.Update(file);
    }
}