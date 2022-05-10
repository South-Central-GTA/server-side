using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Mdc;

public class BulletInEntryModel
    : ModelBase, IWritable
{
    public BulletInEntryModel()
    {
        
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    public string CreatorCharacterName { get; set; }
    
    public string Content { get; set; }
    public FactionType FactionType { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("content");
        writer.Value(Content);

        writer.Name("creatorCharacterName");
        writer.Value(CreatorCharacterName);
        
        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.EndObject();
    }
}