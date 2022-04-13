using System;

namespace Server.Database.Enums;

[Flags]
public enum BankingPermission
{
    NONE = 0,
    DEPOSIT = 1 << 0,
    WITHDRAW = 1 << 1,
    TRANSFER = 1 << 2,
    SEE_HISTORY = 1 << 3,
    MANAGEMENT = 1 << 4
}