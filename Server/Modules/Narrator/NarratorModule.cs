using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Database.Enums;
using Server.Modules.Chat;

namespace Server.Modules.Narrator;

public class NarratorModule : ISingletonScript
{
    private readonly ChatModule _chatModule;

    public NarratorModule(ChatModule chatModule)
    {
        _chatModule = chatModule;
    }

    public void SendMessage(ServerPlayer player, string content)
    {
        _chatModule.SendMessage(player, "Erzähler: ", ChatType.EMOTE, content, "#C2A2DA");
    }
}