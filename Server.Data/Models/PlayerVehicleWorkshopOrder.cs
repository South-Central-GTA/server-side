using Server.Data.Enums;

namespace Server.Data.Models;

public class PlayerVehicleWorkshopOrder
{
    public VehicleModType Type { get; set; }
    public int Value { get; set; }
}