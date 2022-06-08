using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.FileSystem;

namespace Server.Handlers.FileSystem;

public class OpenRootDirectoryHandler : ISingletonScript
{
    private readonly FileModule _fileModule;

    public OpenRootDirectoryHandler(FileModule fileModule)
    {
        _fileModule = fileModule;
        AltAsync.OnClient<ServerPlayer, int>("filesystem:openrootdirectory", OnRequestOpenRootDirectory);
    }

    private async void OnRequestOpenRootDirectory(ServerPlayer player, int groupId)
    {
        if (!player.Exists)
        {
            return;
        }

        await _fileModule.OpenFileSystem(player, groupId);
    }
}