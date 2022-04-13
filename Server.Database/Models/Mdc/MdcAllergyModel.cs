using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.Mdc;

public class MdcAllergyModel
    : ModelBase, IWritable
{
    public MdcAllergyModel()
    {
        
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int CharacterModelId { get; set; }
    public CharacterModel CharacterModel { get; set; }
    
    public string Content { get; set; }
    public string CreatorCharacterName { get; set; }

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