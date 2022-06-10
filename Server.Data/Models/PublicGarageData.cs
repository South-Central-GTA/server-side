using AltV.Net;

namespace Server.Data.Models;

public class PublicGarageData : IWritable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float CostsPercentageOfVehiclePrice { get; set; }
    public float ParkingPointX { get; set; }
    public float ParkingPointY { get; set; }
    public float ParkingPointZ { get; set; }

    public string PedModel { get; set; }
    public float PedPointX { get; set; }
    public float PedPointY { get; set; }
    public float PedPointZ { get; set; }
    public float PedHeading { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("costsPercentageOfVehiclePrice");
        writer.Value(CostsPercentageOfVehiclePrice);

        writer.Name("parkingPointX");
        writer.Value(ParkingPointX);

        writer.Name("parkingPointY");
        writer.Value(ParkingPointY);

        writer.Name("parkingPointZ");
        writer.Value(ParkingPointZ);

        writer.Name("pedPointX");
        writer.Value(PedPointX);

        writer.Name("pedPointY");
        writer.Value(PedPointY);

        writer.Name("pedPointZ");
        writer.Value(PedPointZ);

        writer.Name("pedHeading");
        writer.Value(PedHeading);

        writer.EndObject();
    }
}