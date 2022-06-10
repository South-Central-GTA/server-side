using AltV.Net;

namespace Server.Data.Models;

public class CallSignData : IWritable
{
    public int Id { get; set; }
    public string CallSign { get; set; }
    public string Names { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("callSign");
        writer.Value(CallSign);

        writer.Name("names");
        writer.Value(Names);

        writer.EndObject();
    }
}