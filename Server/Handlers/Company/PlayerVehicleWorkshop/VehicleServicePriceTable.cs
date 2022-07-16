using AltV.Net;

namespace Server.Handlers.Company.PlayerVehicleWorkshop;

public class VehicleServicePriceTable : IWritable
{
    public int PlateVanity { get; set; } = 1;
    public int Dashboard { get; set; } = 1;
    public int DoorSpeaker { get; set; } = 1;
    public int Seats { get; set; } = 1;
    public int Speaker { get; set; } = 1;
    public int Trunk { get; set; } = 1;
    public int EngineBlock { get; set; } = 1;
    public int BoostOrAirFilter { get; set; } = 1;
    public int Struts { get; set; } = 1;
    public int ArchCover { get; set; } = 1;
    public int Aerials { get; set; } = 1;
    public int Trim { get; set; } = 1;
    public int Tank { get; set; } = 1;
    public int Windows { get; set; } = 1;
    public int Spoilers { get; set; } = 1;
    public int FrontBumper { get; set; } = 1;
    public int RearBumper { get; set; } = 1;
    public int SideSkirt { get; set; } = 1;
    public int Exhaust { get; set; } = 1;
    public int Frame { get; set; } = 1;
    public int Grille { get; set; } = 1;
    public int Hood { get; set; } = 1;
    public int Fender { get; set; } = 1;
    public int RightFender { get; set; } = 1;
    public int Roof { get; set; } = 1;
    public int Transmission { get; set; } = 1;
    public int Horns { get; set; } = 1;
    public int Suspension { get; set; } = 1;
    public int Armor { get; set; } = 1;
    public int Turbo { get; set; } = 1;
    public int Xenon { get; set; } = 1;
    public int FrontWheels { get; set; } = 1;
    public int BackWheels { get; set; } = 1;
    public int PlateHolders { get; set; } = 1;
    public int TrimDesign { get; set; } = 1;
    public int Ornaments { get; set; } = 1;
    public int DialDesign { get; set; } = 1;
    public int SteeringWheel { get; set; } = 1;
    public int ShiftLever { get; set; } = 1;
    public int Plaques { get; set; } = 1;
    public int Hydraulics { get; set; } = 1;
    public int Boost { get; set; } = 1;
    public int WindowTint { get; set; } = 1;
    public int Livery { get; set; } = 1;
    public int Plate { get; set; } = 1;
    public int Colour1 { get; set; } = 1;
    public int Colour2 { get; set; } = 1;
    public int[] Engine { get; set; } = { 2500, 5000, 7500, 10000 };
    public int[] Brakes { get; set; } = { 1000, 2500, 5000, 10000 };
    

    public static void Serialize(VehicleServicePriceTable data, IMValueWriter writer)
    {
        writer.BeginObject();
        
        writer.Name("plateVanity");
        writer.Value(data.PlateVanity);
        writer.Name("dashboard");
        writer.Value(data.Dashboard);
        writer.Name("doorSpeaker");
        writer.Value(data.DoorSpeaker);
        writer.Name("seats");
        writer.Value(data.Seats);
        writer.Name("speaker");
        writer.Value(data.Speaker);
        writer.Name("trunk");
        writer.Value(data.Trunk);
        writer.Name("engineBlock");
        writer.Value(data.EngineBlock);
        writer.Name("boostOrAirFilter");
        writer.Value(data.BoostOrAirFilter);
        writer.Name("struts");
        writer.Value(data.Struts);
        writer.Name("archCover");
        writer.Value(data.ArchCover);
        writer.Name("aerials");
        writer.Value(data.Aerials);
        writer.Name("trim");
        writer.Value(data.Trim);
        writer.Name("tank");
        writer.Value(data.Tank);
        writer.Name("windows");
        writer.Value(data.Windows);
        writer.Name("spoilers");
        writer.Value(data.Spoilers);
        writer.Name("frontBumper");
        writer.Value(data.FrontBumper);
        writer.Name("rearBumper");
        writer.Value(data.RearBumper);
        writer.Name("sideSkirt");
        writer.Value(data.SideSkirt);
        writer.Name("exhaust");
        writer.Value(data.Exhaust);
        writer.Name("frame");
        writer.Value(data.Frame);
        writer.Name("grille");
        writer.Value(data.Grille);
        writer.Name("hood");
        writer.Value(data.Hood);
        writer.Name("fender");
        writer.Value(data.Fender);
        writer.Name("rightFender");
        writer.Value(data.RightFender);
        writer.Name("roof");
        writer.Value(data.Roof);
        writer.Name("transmission");
        writer.Value(data.Transmission);
        writer.Name("horns");
        writer.Value(data.Horns);
        writer.Name("suspension");
        writer.Value(data.Suspension);
        writer.Name("armor");
        writer.Value(data.Armor);
        writer.Name("turbo");
        writer.Value(data.Turbo);
        writer.Name("xenon");
        writer.Value(data.Xenon);
        writer.Name("frontWheels");
        writer.Value(data.FrontWheels);
        writer.Name("backWheels");
        writer.Value(data.BackWheels);
        writer.Name("plateholders");
        writer.Value(data.PlateHolders);
        writer.Name("trimDesign");
        writer.Value(data.TrimDesign);
        writer.Name("ornaments");
        writer.Value(data.Ornaments);
        writer.Name("dialDesign");
        writer.Value(data.DialDesign);
        writer.Name("steeringWheel");
        writer.Value(data.SteeringWheel);
        writer.Name("shiftLever");
        writer.Value(data.ShiftLever);
        writer.Name("plaques");
        writer.Value(data.Plaques);
        writer.Name("hydraulics");
        writer.Value(data.Hydraulics);
        writer.Name("boost");
        writer.Value(data.Boost);
        writer.Name("windowTint");
        writer.Value(data.WindowTint);
        writer.Name("livery");
        writer.Value(data.Livery);
        writer.Name("plate");
        writer.Value(data.Plate);
        writer.Name("colour1");
        writer.Value(data.Colour1);
        writer.Name("colour2");
        writer.Value(data.Colour2);
        
        writer.Name("engine");
        writer.BeginArray();

        foreach (var value in data.Engine)
        {
            writer.Value(value);
        }

        writer.EndArray();
        
        writer.Name("brakes");
        writer.BeginArray();

        foreach (var value in data.Brakes)
        {
            writer.Value(value);
        }

        writer.EndArray();
        
        writer.EndObject();
    }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }
}