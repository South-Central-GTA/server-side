using AltV.Net;

namespace Server.Data.Models;

public class HelpMeTicketData : IWritable
{
    public string CreatorName { get; set; }
    public ulong CreatorDiscordId { get; set; }
    public string Context { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("creatorName");
        writer.Value(CreatorName);

        writer.Name("creatorDiscordId");
        writer.Value(CreatorDiscordId.ToString());

        writer.Name("context");
        writer.Value(Context);

        writer.EndObject();
    }
}