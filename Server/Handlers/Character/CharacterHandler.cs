using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;

namespace Server.Handlers.Character;

public class CharacterHandler : ISingletonScript
{
    public CharacterHandler()
    {
        AltAsync.OnClient<ServerPlayer>("character:requestmenu", OnRequestMenu);
    }

    private async void OnRequestMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.EmitLocked("character:showmenu");
    }
}