using System;using Server.Core.Abstractions.ScriptStrategy;
using Server.Data.Enums;
using Server.Database.Models.Vehicles;

namespace Server.Modules.Vehicles;

public class TuningModule : ITransientScript
{
    public void TuneVehicle(PlayerVehicleModel playerVehicleModel, VehicleModType type, int value)
    {
        switch (type)
        {
            case VehicleModType.Spoilers:
                playerVehicleModel.Spoilers = value;
                break;
            case VehicleModType.FrontBumper:
                playerVehicleModel.FrontBumper = value;
                break;
            case VehicleModType.RearBumper:
                playerVehicleModel.RearBumper = value;
                break;
            case VehicleModType.SideSkirt:
                playerVehicleModel.SideSkirt = value;
                break;
            case VehicleModType.Exhaust:
                playerVehicleModel.Exhaust = value;
                break;
            case VehicleModType.Frame:
                playerVehicleModel.Frame = value;
                break;
            case VehicleModType.Grille:
                playerVehicleModel.Grille = value;
                break;
            case VehicleModType.Hood:
                playerVehicleModel.Hood = value;
                break;
            case VehicleModType.Fender:
                playerVehicleModel.Fender = value;
                break;
            case VehicleModType.RightFender:
                playerVehicleModel.RightFender = value;
                break;
            case VehicleModType.Roof:
                playerVehicleModel.Roof = value;
                break;
            case VehicleModType.Engine:
                playerVehicleModel.Engine = value;
                break;
            case VehicleModType.Brakes:
                playerVehicleModel.Brakes = value;
                break;
            case VehicleModType.Transmission:
                playerVehicleModel.Transmission = value;
                break;
            case VehicleModType.Horns:
                playerVehicleModel.Horns = value;
                break;
            case VehicleModType.Suspension:
                playerVehicleModel.Suspension = value;
                break;
            case VehicleModType.Armor:
                playerVehicleModel.Armor = value;
                break;
            case VehicleModType.Turbo:
                playerVehicleModel.Turbo = value;
                break;
            case VehicleModType.Xenon:
                playerVehicleModel.Xenon = value;
                break;
            case VehicleModType.FrontWheels:
                playerVehicleModel.FrontWheels = value;
                break;
            case VehicleModType.BackWheels:
                playerVehicleModel.BackWheels = value;
                break;
            case VehicleModType.PlateHolder:
                playerVehicleModel.PlateHolder = value;
                break;
            case VehicleModType.PlateVanity:
                playerVehicleModel.PlateVanity = value;
                break;
            case VehicleModType.TrimDesign:
                playerVehicleModel.TrimDesign = value;
                break;
            case VehicleModType.Ornaments:
                playerVehicleModel.Ornaments = value;
                break;
            case VehicleModType.Dashboard:
                playerVehicleModel.Dashboard = value;
                break;
            case VehicleModType.DialDesign:
                playerVehicleModel.DialDesign = value;
                break;
            case VehicleModType.DoorSpeaker:
                playerVehicleModel.DoorSpeaker = value;
                break;
            case VehicleModType.Seats:
                playerVehicleModel.Seats = value;
                break;
            case VehicleModType.SteeringWheel:
                playerVehicleModel.SteeringWheel = value;
                break;
            case VehicleModType.ShiftLever:
                playerVehicleModel.ShiftLever = value;
                break;
            case VehicleModType.Plaques:
                playerVehicleModel.Plaques = value;
                break;
            case VehicleModType.Speaker:
                playerVehicleModel.Speaker = value;
                break;
            case VehicleModType.Trunk:
                playerVehicleModel.Trunk = value;
                break;
            case VehicleModType.Hydraulics:
                playerVehicleModel.Hydraulics = value;
                break;
            case VehicleModType.EngineBlock:
                playerVehicleModel.EngineBlock = value;
                break;
            case VehicleModType.BoostOrAirFilter:
                playerVehicleModel.AirFilter = value;
                break;
            case VehicleModType.Struts:
                playerVehicleModel.Struts = value;
                break;
            case VehicleModType.ArchCover:
                playerVehicleModel.ArchCover = value;
                break;
            case VehicleModType.Aerials:
                playerVehicleModel.Aerials = value;
                break;
            case VehicleModType.Trim:
                playerVehicleModel.Trim = value;
                break;
            case VehicleModType.Tank:
                playerVehicleModel.Tank = value;
                break;
            case VehicleModType.Windows:
                playerVehicleModel.Windows = value;
                break;
            case VehicleModType.WindowTint:
                playerVehicleModel.WindowTint = value;
                break;
            case VehicleModType.Livery:
                playerVehicleModel.Livery = (byte)value;
                break;
            case VehicleModType.Plate:
                playerVehicleModel.Plate = value;
                break;
            case VehicleModType.Colour1:
                playerVehicleModel.PrimaryColor = value;
                break;
            case VehicleModType.Colour2:
                playerVehicleModel.SecondaryColor = value;
                break;
            case VehicleModType.Repair:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}