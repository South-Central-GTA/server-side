using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class OpenFileHandler : ISingletonScript
{
    private readonly FileService _fileService;

    private readonly GroupModule _groupModule;

    public OpenFileHandler(FileService fileService, GroupModule groupModule)
    {
        _fileService = fileService;

        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("filesystem:requestfile", OnRequestOpenFile);
    }

    private async void OnRequestOpenFile(ServerPlayer player, int fileId)
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

        if (groupMember.RankLevel < file.DirectoryModel.ReadGroupLevel && !groupMember.Owner)
        {
            return;
        }

        player.EmitGui("filesystem:openfile", file);
    }
}