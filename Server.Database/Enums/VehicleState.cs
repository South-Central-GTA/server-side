namespace Server.Database.Enums;

public enum VehicleState
{
    SPAWNED,
    IN_GARAGE,
    IN_STORAGE, // only for vehicle dealer companies
    DESTROYED
}