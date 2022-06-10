using AltV.Net;
using Server.Database.Enums;

namespace Server.Data.Models;

public class ChatMessageData : IWritable
{
    public string? Sender { get; set; }
    public string Context { get; set; }
    public string AfterName { get; set; }
    public string BeforeChat { get; set; }
    public string AfterChat { get; set; }
    public string NameColor { get; set; }
    public string Color { get; set; }
    public string BackgroundColor { get; set; }
    public string SendetAt { get; set; }
    public ChatType ChatType { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("sender");
        writer.Value(Sender);

        writer.Name("context");
        writer.Value(Context);

        writer.Name("afterName");
        writer.Value(AfterName);

        writer.Name("beforeChat");
        writer.Value(BeforeChat);

        writer.Name("afterChat");
        writer.Value(AfterChat);

        writer.Name("nameColor");
        writer.Value(NameColor);

        writer.Name("color");
        writer.Value(Color);

        writer.Name("bgColor");
        writer.Value(BackgroundColor);

        writer.Name("sendetAt");
        writer.Value(SendetAt);

        writer.Name("chatType");
        writer.Value((int)ChatType);

        writer.EndObject();
    }
}