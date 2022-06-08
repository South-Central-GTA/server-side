using System;
using System.Text.Json;
using AltV.Net;

namespace Server.Data.Models;

public class FileData
    : IWritable
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Context { get; set; }
    public bool IsDirectory { get; set; }
    public string LastEditCharacterName { get; set; }
    public int CreatorCharacterId { get; set; }
    public string CreatorCharacterName { get; set; }
    public DateTime LastEdit { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("title");
        writer.Value(Title);

        writer.Name("isDirectory");
        writer.Value(IsDirectory);

        writer.Name("context");
        writer.Value(Context);

        writer.Name("creatorCharacterId");
        writer.Value(CreatorCharacterId);

        writer.Name("creatorCharacterName");
        writer.Value(CreatorCharacterName);

        writer.Name("lastEditCharacterName");
        writer.Value(LastEditCharacterName);

        writer.Name("lastEditAtJson");
        writer.Value(JsonSerializer.Serialize(LastEdit));

        writer.EndObject();
    }
}