using System.Numerics;
using AltV.Net.Data;
using AltV.Net.EntitySync;
using Server.Data.Enums;
using Server.Data.Enums.EntitySync;

namespace Server.Core.Entities;

public class ServerBlip : Entity
{
    public ServerBlip(Vector3 position, int dimension, uint range) : base((ulong)EntityType.BLIP, position, dimension, range)
    {
    }

    /// <summary>
    ///     The text to display on the blip in the map menu
    /// </summary>
    public string? Name
    {
        get
        {
            if (!TryGetData("name", out string name))
            {
                return null;
            }

            return name;
        }
        set => SetData("name", value);
    }

    /// <summary>
    ///     The blip type
    /// </summary>
    public BlipType BlipType
    {
        get
        {
            if (!TryGetData("blipType", out BlipType blipType))
            {
                return BlipType.POINT;
            }

            return blipType;
        }
        set => SetData("blipType", (int)value);
    }

    /// <summary>
    ///     If this player is set the blip will only show up for this specific player.
    /// </summary>
    public ServerPlayer? Player
    {
        get
        {
            if (!TryGetData("player", out ServerPlayer player))
            {
                return null;
            }

            return player;
        }
        set => SetData("player", value);
    }

    /// <summary>
    ///     ID of the sprite to use, can be found on the ALTV wiki
    /// </summary>
    public int Sprite
    {
        get
        {
            if (!TryGetData("sprite", out int spriteId))
            {
                return 0;
            }

            return spriteId;
        }
        set => SetData("sprite", value);
    }

    public int Radius
    {
        get
        {
            if (!TryGetData("radius", out int radius))
            {
                return 0;
            }

            return radius;
        }
        set => SetData("radius", value);
    }

    public int Alpha
    {
        get
        {
            if (!TryGetData("alpha", out int alpha))
            {
                return 255;
            }

            return alpha;
        }
        set => SetData("alpha", value);
    }

    /// <summary>
    ///     Blip Color code, can also be found on the ALTV wiki
    /// </summary>
    public int Color
    {
        get
        {
            if (!TryGetData("color", out int color))
            {
                return 0;
            }

            return color;
        }
        set => SetData("color", value);
    }

    /// <summary>
    ///     Scale of the blip, 1 is regular size.
    /// </summary>
    public float Scale
    {
        get
        {
            if (!TryGetData("scale", out float scale))
            {
                return 1;
            }

            return scale;
        }
        set => SetData("scale", value);
    }

    /// <summary>
    ///     Whether this blip can be seen on the minimap from anywhere on the map, or only when close to it(it will always show
    ///     on the main map).
    /// </summary>
    public bool ShortRange
    {
        get
        {
            if (!TryGetData("shortRange", out bool shortRange))
            {
                return true;
            }

            return shortRange;
        }
        set => SetData("shortRange", value);
    }

    public void SetPosition(Position position)
    {
        Position = position;
    }
}