namespace Server.Core.Configuration;

public class VehicleOptions
{
    public int ProductPriceFullRepair { get; set; }
    public int VehicleFuelInterval { get; set; }
    public float VehicleFuelDefaultReduction { get; set; }
    public float VehicleFuelVelocityMultiplier { get; set; }
    public int EngineDamageUntilMoreFuelCosts { get; set; }
    public int DrivenKilometerUntilMorFuelCosts { get; set; }
}