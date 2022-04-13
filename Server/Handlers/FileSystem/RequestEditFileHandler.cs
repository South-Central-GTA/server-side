using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class RequestEditFileHandler : ISingletonScript
{
    private readonly FileService _fileService;

    private readonly GroupModule _groupModule;
    
    public RequestEditFileHandler(
        FileService fileService,
        
        GroupModule groupModule)
    {
        _fileService = fileService;
        
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("filesystem:requesteditfile", OnRequestEditFile);
    }

    private async void OnRequestEditFile(ServerPlayer player, int fileId)
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
        
        var groupMember = await _groupModule.GetGroupMember(player, file.DirectoryModel.GroupModelId);
        if (groupMember == null)
        {
            return;
        }
        
        if (groupMember.RankLevel < file.DirectoryModel.WriteGroupLevel && !groupMember.Owner)
        {
            return;
        }

        if (file.IsBlocked && file.BlockedByCharacterName != null)
        {
            player.EmitGui("filesystem:fileblocked", file.BlockedByCharacterName);
            return;
        }

        file.IsBlocked = true;
        file.BlockedByCharacterName = player.CharacterModel.Name;

        await _fileService.Update(file);
        
        player.EmitGui("filesystem:editfile", file.Context);
    }
}