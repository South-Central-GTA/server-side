using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Models.File;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class CreateFileHandler : ISingletonScript
{
    private readonly FileService _fileService;
    private readonly GroupModule _groupModule;

    private readonly SyncFileModule _syncFileModule;

    public CreateFileHandler(FileService fileService, SyncFileModule syncFileModule, GroupModule groupModule)
    {
        _fileService = fileService;

        _syncFileModule = syncFileModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, int>("filesystem:createfile", OnCreateFile);
    }

    private async void OnCreateFile(ServerPlayer player, int groupId, int directoryId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!await _groupModule.IsPlayerInGroup(player, groupId))
        {
            return;
        }

        await _fileService.Add(new FileModel
        {
            Title = "Neue Textdatei",
            DirectoryModelId = directoryId,
            CreatorCharacterId = player.CharacterModel.Id,
            CreatorCharacterName = player.CharacterModel.Name,
            LastEditCharacterName = player.CharacterModel.Name
        });

        await _syncFileModule.UpdateDirectory(groupId, directoryId);
    }
}