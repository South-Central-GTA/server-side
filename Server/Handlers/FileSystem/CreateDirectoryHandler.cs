using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Models.File;
using Server.Modules.FileSystem;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class CreateDirectoryHandler : ISingletonScript
{
    private readonly DirectoryService _directoryService;

    private readonly SyncFileModule _syncFileModule;
    private readonly GroupModule _groupModule;

    public CreateDirectoryHandler(
        DirectoryService directoryService,
        SyncFileModule syncFileModule,
        GroupModule groupModule)
    {
        _directoryService = directoryService;

        _syncFileModule = syncFileModule;
        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int>("filesystem:createdirectory", OnCreateDirectory);
    }

    private async void OnCreateDirectory(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!await _groupModule.IsPlayerInGroup(player, groupId))
        {
            return;
        }

        await _directoryService.Add(new DirectoryModel()
        {
            Title = "Neuer Ordner",
            GroupModelId = groupId,
            CreatorCharacterName = player.CharacterModel.Name,
            LastEditCharacterName = player.CharacterModel.Name,
        });

        await _syncFileModule.UpdateDirectory(groupId);
    }
}