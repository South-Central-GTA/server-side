using System.Collections.Generic;

namespace Server.Core.Configuration;

public class DeliveryOptions
{
    public int ProductPrice { get; set; }
    public float SharesForSuppliers { get; set; }
    public int PickupTime { get; set; }
    public int ResetTime { get; set; }
    public Dictionary<string, int> TransportVehicleMaxProducts { get; set; }
}