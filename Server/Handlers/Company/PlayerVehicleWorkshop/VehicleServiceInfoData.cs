using AltV.Net;

namespace Server.Handlers.Company.PlayerVehicleWorkshop;

public class VehicleServiceInfoData : IWritable
{
    public int ProductCount { get; set; }
    public int CurrentProductPrice { get; set; }
    public int VehiclePrice { get; set; }
    public string VehicleModelName { get; set; }
    public int PrimaryColor { get; set; }
    public int SecondaryColor { get; set; }
    public float VehicleDamagePercentage { get; set; }
    public VehicleServicePriceTable PriceTable { get; set; } = new VehicleServicePriceTable();

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("productCount");
        writer.Value(ProductCount);
        
        writer.Name("currentProductPrice");
        writer.Value(CurrentProductPrice);
        
        writer.Name("vehiclePrice");
        writer.Value(VehiclePrice);
        
        writer.Name("vehicleModelName");
        writer.Value(VehicleModelName);
        
        writer.Name("primaryColor");
        writer.Value(PrimaryColor);
        
        writer.Name("secondaryColor");
        writer.Value(SecondaryColor);
        
        writer.Name("vehicleDamagePercentage");
        writer.Value(VehicleDamagePercentage);

        writer.Name("priceTable");
        VehicleServicePriceTable.Serialize(PriceTable, writer);

        writer.EndObject();
    }
}