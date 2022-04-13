namespace Server.Data.Models;

public class DrivingSchoolData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DrivingInstructor { get; set; }
    public int DrivingLicensePrice { get; set; }

    public float StartPointX { get; set; }
    public float StartPointY { get; set; }
    public float StartPointZ { get; set; }


    public float StartPointRoll { get; set; }
    public float StartPointPitch { get; set; }
    public float StartPointYaw { get; set; }

    public string PedModel { get; set; }
    public float PedPointX { get; set; }
    public float PedPointY { get; set; }
    public float PedPointZ { get; set; }
    public float PedHeading { get; set; }
    public int VehPrimColor { get; set; }
    public int VehSecColor { get; set; }
    public string[] VehicleModels { get; set; }
}