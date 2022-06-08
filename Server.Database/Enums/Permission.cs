﻿using System;

namespace Server.Database.Enums;

[Flags]
public enum Permission
{
    NONE = 0,
    TESTER = (1 << 0),

    STAFF = TESTER | (1 << 1),

    FACTION_MANAGEMENT = TESTER | STAFF | (1 << 2),
    HEAD_FACTION_MANAGEMENT = TESTER | STAFF | (1 << 3),

    EVENT_MANAGEMENT = TESTER | STAFF | (1 << 4),
    HEAD_EVENT_MANAGEMENT = TESTER | STAFF | EVENT_MANAGEMENT | (1 << 5),

    TEAM_MANAGEMENT = TESTER | STAFF | (1 << 6),
    HEAD_TEAM_MANAGEMENT = TESTER | STAFF | TEAM_MANAGEMENT | (1 << 7),

    ECONOMY_MANAGEMENT = TESTER | STAFF | (1 << 8),
    HEAD_ECONOMY_MANAGEMENT = TESTER | STAFF | ECONOMY_MANAGEMENT | (1 << 9),

    LORE_AND_EVENT_MANAGEMENT = TESTER | STAFF | (1 << 10),
    HEAD_LORE_AND_EVENT_MANAGEMENT = TESTER | STAFF | LORE_AND_EVENT_MANAGEMENT | (1 << 11),

    MANAGE_ANIMATIONS = (1 << 12),

    MOD = STAFF | (1 << 13),
    ADMIN = MOD | (1 << 14),
    DEV = ADMIN | (1 << 15),

    LEAD_AMIN = ADMIN |
                HEAD_FACTION_MANAGEMENT |
                HEAD_EVENT_MANAGEMENT |
                HEAD_TEAM_MANAGEMENT |
                HEAD_ECONOMY_MANAGEMENT |
                HEAD_LORE_AND_EVENT_MANAGEMENT |
                MANAGE_ANIMATIONS | (1 << 16),

    FOUNDER = (1 << 17),

    OWNER = LEAD_AMIN |
            HEAD_FACTION_MANAGEMENT |
            HEAD_EVENT_MANAGEMENT |
            HEAD_TEAM_MANAGEMENT |
            HEAD_ECONOMY_MANAGEMENT |
            HEAD_LORE_AND_EVENT_MANAGEMENT |
            MANAGE_ANIMATIONS | (1 << 18)
}