using AltV.Net;

namespace Server.Data.Models;

public class PlayerInformationData
    : IWritable
{
    public int Id { get; set; }
    public ulong AccountId { get; set; }
    public string AccountName { get; set; }
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public ulong DiscordId { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("accountId");
        writer.Value(AccountId);

        writer.Name("accountName");
        writer.Value(AccountName);

        writer.Name("characterId");
        writer.Value(CharacterId);
        
        writer.Name("characterName");
        writer.Value(CharacterName);

        writer.Name("discordId");
        writer.Value(DiscordId.ToString());

        writer.EndObject();
    }
}