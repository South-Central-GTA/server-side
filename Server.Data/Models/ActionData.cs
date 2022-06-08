using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;

namespace Server.Data.Models;

public class ActionData
    : IWritable
{
    public ActionData(string title, string eventName, object? customData = null)
    {
        Title = title;
        Event = eventName;
        CustomData = customData;
    }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("event")] public string Event { get; set; }

    [JsonPropertyName("customData")] public object? CustomData { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("title");
        writer.Value(Title);

        writer.Name("event");
        writer.Value(Event);

        writer.Name("customData");
        writer.Value(JsonSerializer.Serialize(CustomData));

        writer.EndObject();
    }
}