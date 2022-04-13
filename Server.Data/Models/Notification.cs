using AltV.Net;
using Server.Data.Enums;

namespace Server.Data.Models;

public class Notification
    : IWritable
{
    public Notification(NotificationType type, string text)
    {
        Type = type;
        Text = text;
    }

    public NotificationType Type { get; }
    public string Text { get; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("type");
        writer.Value((int)Type);

        writer.Name("text");
        writer.Value(Text);

        writer.EndObject();
    }
}