using System;

namespace Server.Database.Enums;

[Flags]
public enum ItemFlags
{
    NONE = 0,
    MONEY = 1 << 0,
    CLOTHINGS = 1 << 1,
    CONSUMEABLE = 1 << 2
}