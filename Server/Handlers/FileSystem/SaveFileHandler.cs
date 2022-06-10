using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.Group;

namespace Server.Handlers.FileSystem;

public class SaveFileHandler : ISingletonScript
{
    private readonly FileService _fileService;

    private readonly GroupModule _groupModule;

    public SaveFileHandler(FileService fileService, GroupModule groupModule)
    {
        _fileService = fileService;

        _groupModule = groupModule;

        AltAsync.OnClient<ServerPlayer, int, string>("filesystem:savefile", OnSaveFile);
    }

    private async void OnSaveFile(ServerPlayer player, int fileId, string context)
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

        file.Context = context;
        file.LastEditCharacterName = player.CharacterModel.Name;
        file.IsBlocked = false;

        await _fileService.Update(file);

        player.EmitGui("filesystem:filesaved");
    }
}