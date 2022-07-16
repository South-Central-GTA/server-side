namespace Server.Data.Models;

public class VehicleServiceData
{
    public int VehicleDbId { get; set; }
    public PlayerVehicleWorkshopOrder[] Orders { get; set; }
}