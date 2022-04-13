using System;
using System.Collections.Generic;

namespace Server.Core.Extensions;

public static class ListExtensions
{
    private static readonly Random Random = new();

    public static void AddRangeUnique<T>(this IList<T> self, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            if (!self.Contains(item))
            {
                self.Add(item);
            }
        }
    }

    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = Random.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static List<List<T>> Split<T>(this IEnumerable<T> collection, int size)
    {
        var chunks = new List<List<T>>();
        var count = 0;
        var temp = new List<T>();

        foreach (var element in collection)
        {
            if (count++ == size)
            {
                chunks.Add(temp);
                temp = new List<T>();
                count = 1;
            }

            temp.Add(element);
        }

        chunks.Add(temp);

        return chunks;
    }
}