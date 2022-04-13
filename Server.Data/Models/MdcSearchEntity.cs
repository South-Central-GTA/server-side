using System.Text.Json.Serialization;
using AltV.Net;
using Server.Data.Enums;
using Server.Database.Enums;

namespace Server.Data.Models;

public class MdcSearchEntity
    : IWritable
{
    [JsonPropertyName("id")] 
    public int Id { get; set; }
    public string StringId { get; set; }

    [JsonPropertyName("name")] 
    public string Name { get; set; }

    [JsonPropertyName("type")] 
    public MdcSearchType Type { get; set; }
    

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);
        
        writer.Name("stringId");
        writer.Value(StringId);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("type");
        writer.Value((int)Type);

        writer.EndObject();
    }
}