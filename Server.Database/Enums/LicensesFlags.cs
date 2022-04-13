using System;

namespace Server.Database.Enums;

[Flags]
public enum LicensesFlags
{
    NONE = 0,
    VEHICLE_WORKSHOP = 1 << 0,
    GOODS_TRANSPORT = 1 << 1,
    PERSON_TRANSPORT = 1 << 2,
    TOWING_AGENCY = 1 << 3,
    VEHICLE_DEALERSHIP = 1 << 4
}