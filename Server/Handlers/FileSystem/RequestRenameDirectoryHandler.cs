using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class RequestRenameDirectoryHandler : ISingletonScript
{
    private readonly DirectoryService _directoryService;

    private readonly GroupModule _groupModule;
    private readonly SyncFileModule _syncFileModule;

    public RequestRenameDirectoryHandler(
        DirectoryService directoryService,
        GroupModule groupModule,
        SyncFileModule syncFileModule)
    {
        _directoryService = directoryService;

        _groupModule = groupModule;
        _syncFileModule = syncFileModule;

        AltAsync.OnClient<ServerPlayer, int, string>("filesystem:requestrenamedirectory", OnRequestDirectoryName);
    }

    private async void OnRequestDirectoryName(ServerPlayer player, int directoryId, string title)
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

        var groupMember = await _groupModule.GetGroupMember(player, directory.GroupModelId);
        if (groupMember == null)
        {
            return;
        }

        directory.Title = title;
        directory.LastEditCharacterName = player.CharacterModel.Name;

        await _directoryService.Update(directory);
        await _syncFileModule.UpdateDirectory(groupMember.GroupModelId);
    }
}