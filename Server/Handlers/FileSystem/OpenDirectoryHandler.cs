using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class OpenDirectoryHandler : ISingletonScript
{
    private readonly DirectoryService _directoryService;

    private readonly FileModule _fileModule;
    private readonly GroupModule _groupModule;

    public OpenDirectoryHandler(
        DirectoryService directoryService,
        FileModule fileModule,
        GroupModule groupModule)
    {
        _directoryService = directoryService;

        _fileModule = fileModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("filesystem:requestdirectory", OnRequestOpenDirectory);
    }

    private async void OnRequestOpenDirectory(ServerPlayer player, int directoryId)
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

        if (groupMember.RankLevel < directory.ReadGroupLevel && !groupMember.Owner)
        {
            return;
        }

        player.SetData("FILE_SYSTEM_DIRECTORY", directoryId);

        player.EmitGui("filesystem:opendirectory",
                       directoryId,
                       directory.Title,
                       await _fileModule.GetAllFilesFromDirectory(directoryId));
    }
}