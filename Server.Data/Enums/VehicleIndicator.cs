using System;

namespace Server.Data.Enums;

[Flags]
public enum VehicleIndicator
{
    NONE = 0x00,
    RIGHT = 0x01,
    LEFT = 0x10,
    HAZARD = LEFT | RIGHT
}