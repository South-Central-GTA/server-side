using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Models;
using Server.DataAccessLayer.Services;

namespace Server.Modules.FileSystem;

public class FileModule : ISingletonScript
{
    private readonly DirectoryService _directoryService;

    public FileModule(DirectoryService directoryService)
    {
        _directoryService = directoryService;
    }

    public async Task OpenFileSystem(ServerPlayer player, int groupId)
    {
        if (player.HasData("FILE_SYSTEM_DIRECTORY"))
        {
            player.DeleteData("FILE_SYSTEM_DIRECTORY");
        }

        player.EmitGui("filesystem:opendirectory", null, null, await GetAllDirectories(groupId));
    }

    public async Task<List<FileData>> GetAllDirectories(int groupId)
    {
        var directories = await _directoryService.Where(d => d.GroupModelId == groupId);

        return directories.Select(directory => new FileData
        {
            Id = directory.Id,
            IsDirectory = true,
            Title = directory.Title,
            LastEdit = directory.LastUsage,
            CreatorCharacterId = directory.CreatorCharacterId,
            CreatorCharacterName = directory.CreatorCharacterName,
            LastEditCharacterName = directory.LastEditCharacterName
        }).ToList();
    }

    public async Task<List<FileData>> GetAllFilesFromDirectory(int directoryId)
    {
        var mdcFiles = new List<FileData>();

        var directory = await _directoryService.Find(d => d.Id == directoryId);

        if (directory != null)
        {
            mdcFiles.AddRange(directory.Files.Select(file => new FileData
            {
                Id = file.Id,
                IsDirectory = false,
                Title = file.Title,
                LastEdit = file.LastUsage,
                CreatorCharacterId = file.CreatorCharacterId,
                CreatorCharacterName = file.CreatorCharacterName,
                LastEditCharacterName = file.LastEditCharacterName
            }));
        }

        return mdcFiles;
    }
}