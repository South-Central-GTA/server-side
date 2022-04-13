using AltV.Net.Data;

namespace Server.Data.Dumps;

public class VehicleDumpEntry
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public long Hash { get; set; }
    public long SignedHash { get; set; }
    public string HexHash { get; set; }
    public string DlcName { get; set; }
    public string HandlingId { get; set; }
    public string LayoutId { get; set; }
    public string Manufacturer { get; set; }
    public string ManufacturerDisplayName { get; set; }
    public string Class { get; set; }
    public string Type { get; set; }
    public string PlateType { get; set; }
    public object DashboardType { get; set; }
    public string WheelType { get; set; }
    public string[] Flags { get; set; }
    public long Seats { get; set; }
    public long Price { get; set; }
    public long MonetaryValue { get; set; }
    public bool HasConvertibleRoof { get; set; }
    public bool HasSirens { get; set; }
    public object[] Weapons { get; set; }
    public string[] ModKits { get; set; }
    public Position DimensionsMin { get; set; }
    public Position DimensionsMax { get; set; }
    public Position BoundingCenter { get; set; }
    public double BoundingSphereRadius { get; set; }
    public object Rewards { get; set; }
    public double MaxBraking { get; set; }
    public double MaxBrakingMods { get; set; }
    public double MaxSpeed { get; set; }
    public double MaxTraction { get; set; }
    public double Acceleration { get; set; }
}