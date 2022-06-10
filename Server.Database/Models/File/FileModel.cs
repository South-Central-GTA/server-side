using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.File;

public class FileModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int DirectoryModelId { get; set; }

    public DirectoryModel DirectoryModel { get; set; }

    [MaxLength(50)] public string Title { get; set; } = "";

    public string Context { get; set; } = "";
    public bool IsBlocked { get; set; }

    public string? BlockedByCharacterName { get; set; }
    public string LastEditCharacterName { get; set; }

    public int CreatorCharacterId { get; set; }
    public string CreatorCharacterName { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(FileModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("title");
        writer.Value(model.Title);

        writer.Name("isDirectory");
        writer.Value(false);

        writer.Name("context");
        writer.Value(model.Context);

        writer.Name("creatorCharacterName");
        writer.Value(model.CreatorCharacterName);

        writer.Name("lastEditCharacterName");
        writer.Value(model.LastEditCharacterName);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}