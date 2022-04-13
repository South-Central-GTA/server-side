using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;

namespace Server.Modules.FileSystem;

public class SyncFileModule : ISingletonScript
{
    private readonly GroupService _groupService;
    private readonly FileModule _fileModule;
    private readonly DirectoryService _directoryService;

    public SyncFileModule(
        GroupService groupService, 
        DirectoryService directoryService,
        
        FileModule fileModule)
    {
        _groupService = groupService;
        _directoryService = directoryService;
        
        _fileModule = fileModule;
    }

    public async Task UpdateDirectory(int groupId, int? directoryId = null)
    {
        var groupModel = await _groupService.GetByKey(groupId);
        if (groupModel == null)
        {
            return;
        }
        
        foreach (var serverPlayer in groupModel.Members
                                               .Select(groupMember => Alt.GetAllPlayers()
                                                                         .FindPlayerByCharacterId(groupMember.CharacterModelId))
                                               .Where(serverPlayer => serverPlayer != null))
        {
            if (directoryId.HasValue)
            {
                if (!serverPlayer.GetData("FILE_SYSTEM_DIRECTORY", out int playerDirectoryId))
                {
                    continue;
                }

                if (directoryId.Value != playerDirectoryId)
                {
                    continue;
                }
                
                var directory = await _directoryService.Find(d => d.Id == directoryId);
                if (directory == null)
                {
                    return;
                }
                
                serverPlayer.EmitGui("filesystem:opendirectory", directoryId.Value, directory.Title,
                                     await _fileModule.GetAllFilesFromDirectory(directoryId.Value));
            }
            else
            {
                serverPlayer.EmitGui("filesystem:opendirectory", null, null, await _fileModule.GetAllDirectories(groupId));
            }
        }
    }
}