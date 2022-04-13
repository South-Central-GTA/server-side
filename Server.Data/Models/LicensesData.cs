using AltV.Net;
using Server.Database.Enums;

namespace Server.Data.Models;

public class LicensesData
    : IWritable
{
    public LicensesFlags License { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("license");
        writer.Value(License.ToString());

        writer.Name("name");
        writer.Value(Name);

        writer.Name("price");
        writer.Value(Price);

        writer.EndObject();
    }
}