using AltV.Net;

namespace Server.Data.Models;

public class PublicGarageEntryData
    : VehicleData, IWritable
{
    public int Costs { get; set; }

    public new void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("costs");
        writer.Value(Costs);

        writer.Name("displayName");
        writer.Value(DisplayName);

        writer.Name("displayClass");
        writer.Value(DisplayClass);

        writer.EndObject();
    }
}