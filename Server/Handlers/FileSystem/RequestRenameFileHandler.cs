using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class RequestRenameFileHandler : ISingletonScript
{
    private readonly FileService _fileService;

    private readonly GroupModule _groupModule;
    private readonly SyncFileModule _syncFileModule;

    public RequestRenameFileHandler(
        FileService fileService,
        GroupModule groupModule,
        SyncFileModule syncFileModule)
    {
        _fileService = fileService;

        _groupModule = groupModule;
        _syncFileModule = syncFileModule;

        AltAsync.OnClient<ServerPlayer, int, string>("filesystem:requestrenamefile", OnRequestFileName);
    }

    private async void OnRequestFileName(ServerPlayer player, int fileId, string title)
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

        var groupMember = await _groupModule.GetGroupMember(player, file.DirectoryModel.GroupModelId);
        if (groupMember == null)
        {
            return;
        }

        file.Title = title;
        file.LastEditCharacterName = player.CharacterModel.Name;

        await _fileService.Update(file);
        await _syncFileModule.UpdateDirectory(groupMember.GroupModelId, file.DirectoryModelId);
    }
}