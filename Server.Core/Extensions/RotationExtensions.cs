using System;
using AltV.Net.Data;

namespace Server.Core.Extensions;

public static class RotationExtensions
{
    public static Position GetDirectionFromRotation(this Rotation rotation)
    {
        var z = rotation.Pitch * (Math.PI / 180.0);
        var x = rotation.Roll * (Math.PI / 180.0);
        var num = Math.Abs(Math.Cos(x));

        return new Position((float)(-Math.Sin(z) * num),
                            (float)(Math.Cos(z) * num),
                            (float)Math.Sin(x));
    }
}