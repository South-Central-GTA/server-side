﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Core.Extensions;

public static class EnumExtensions
{
    public static string GetFlagsAsString(this Enum e, string spacer = " ")
    {
        return string.Join(spacer, Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag));
    }

    public static IEnumerable<Enum> GetFlags(this Enum value)
    {
        return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
    }

    public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
    {
        return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
    }

    private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
    {
        var bits = Convert.ToUInt64(value);
        var results = new List<Enum>();
        for (var i = values.Length - 1; i >= 0; i--)
        {
            var mask = Convert.ToUInt64(values[i]);
            if (i == 0 && mask == 0L)
            {
                break;
            }

            if ((bits & mask) == mask)
            {
                results.Add(values[i]);
                bits -= mask;
            }
        }

        if (bits != 0L)
        {
            return Enumerable.Empty<Enum>();
        }

        if (Convert.ToUInt64(value) != 0L)
        {
            return results.Reverse<Enum>();
        }

        if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
        {
            return values.Take(1);
        }

        return Enumerable.Empty<Enum>();
    }

    private static IEnumerable<Enum> GetFlagValues(Type enumType)
    {
        ulong flag = 0x1;
        foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
        {
            var bits = Convert.ToUInt64(value);
            if (bits == 0L)
                //yield return value;
            {
                continue; // skip the zero value
            }

            while (flag < bits)
            {
                flag <<= 1;
            }

            if (flag == bits)
            {
                yield return value;
            }
        }
    }
}