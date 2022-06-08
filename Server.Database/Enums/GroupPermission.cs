using System;

namespace Server.Database.Enums;

[Flags]
public enum GroupPermission
{
    NONE = 0,
    MANAGE_MEMBERS = 1 << 1,
    INVITE = 1 << 2,
    UNINVITE = 1 << 3,

    CHANGE_DELIVERY_VISIBILITY = 1 << 4,
    ORDER_PRODUCTS = 1 << 5,
    BUY_LICENSES = 1 << 6,
    SELL_LICENSES = 1 << 7,

    SELL_VEHICLES = 1 << 8,
    ORDER_VEHICLES = 1 << 9,
    STORE_VEHICLES = 1 << 10,

    BANKING_DEPOSIT = 1 << 11,
    BANKING_WITHDRAW = 1 << 12,
    BANKING_TRANSFER = 1 << 13,
    BANKING_SEE_HISTORY = 1 << 14,

    MAILING_SENDING = 1 << 15,
    MAILING_READING = 1 << 16,
    MAILING_DELETING = 1 << 17,

    MDC_OPERATOR = 1 << 17
}