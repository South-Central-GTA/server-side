using AltV.Net;

namespace Server.Data.Models;

public class DefinedJobData
    : IWritable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public uint Salary { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("salary");
        writer.Value(Salary);

        writer.EndObject();
    }
}