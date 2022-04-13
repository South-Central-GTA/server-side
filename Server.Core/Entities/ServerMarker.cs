using System;
using System.Collections.Generic;
using System.Numerics;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Data.Enums.EntitySync;
using Entity = AltV.Net.EntitySync.Entity;

namespace Server.Core.Entities;

public class ServerMarker
    : Entity
{
    public ServerMarker(Vector3 position, int dimension, uint range)
        : base((ulong)EntityType.MARKER, position, dimension, range)
    {
    }

    /// <summary>
    ///     Set or get the current marker's type(see MarkerType enum).
    /// </summary>
    public MarkerType MarkerType
    {
        get
        {
            if (!TryGetData("markerType", out int markerType))
            {
                return default;
            }

            return (MarkerType)markerType;
        }
        set
        {
            // No data changed
            if (MarkerType == value)
            {
                return;
            }

            SetData("markerType", (int)value);
        }
    }

    public Vector3 Rotation
    {
        get
        {
            if (!TryGetData("rotation", out Dictionary<string, object> data))
            {
                return default;
            }

            return new Vector3 { X = Convert.ToSingle(data["x"]), Y = Convert.ToSingle(data["y"]), Z = Convert.ToSingle(data["z"]) };
        }
        set
        {
            var dict = new Dictionary<string, object> { ["x"] = value.X, ["y"] = value.Y, ["z"] = value.Z };
            SetData("rotation", dict);
        }
    }

    public Vector3 Direction
    {
        get
        {
            if (!TryGetData("direction", out Dictionary<string, object> data))
            {
                return default;
            }

            return new Vector3 { X = Convert.ToSingle(data["x"]), Y = Convert.ToSingle(data["y"]), Z = Convert.ToSingle(data["z"]) };
        }
        set
        {
            var dict = new Dictionary<string, object> { ["x"] = value.X, ["y"] = value.Y, ["z"] = value.Z };
            SetData("direction", dict);
        }
    }

    public Vector3 Scale
    {
        get
        {
            if (!TryGetData("scale", out Dictionary<string, object> data))
            {
                return default;
            }

            return new Vector3 { X = Convert.ToSingle(data["x"]), Y = Convert.ToSingle(data["y"]), Z = Convert.ToSingle(data["z"]) };
        }
        set
        {
            var dict = new Dictionary<string, object> { ["x"] = value.X, ["y"] = value.Y, ["z"] = value.Z };
            SetData("scale", dict);
        }
    }

    public Rgba Color
    {
        get
        {
            if (!TryGetData("color", out Dictionary<string, object> data))
            {
                return default;
            }

            return new Rgba(Convert.ToByte(data["red"]),
                            Convert.ToByte(data["green"]),
                            Convert.ToByte(data["blue"]),
                            Convert.ToByte(data["alpha"]));
        }
        set
        {
            var dict = new Dictionary<string, object> { { "red", Convert.ToInt32(value.R) }, { "green", Convert.ToInt32(value.G) }, { "blue", Convert.ToInt32(value.B) }, { "alpha", Convert.ToInt32(value.A) } };
            SetData("color", dict);
        }
    }

    public bool BobUpDown
    {
        get => TryGetData("bobUpDown", out bool frozen) && frozen;
        set => SetData("bobUpDown", value);
    }

    public string Text
    {
        get => !TryGetData("text", out string name) ? null : name;
        set
        {
            if (Text == value)
            {
                return;
            }

            SetData("text", value);
        }
    }

    public string OwnerName
    {
        get => !TryGetData("ownerName", out string name) ? null : name;
        set
        {
            if (OwnerName == value)
            {
                return;
            }

            SetData("ownerName", value);
        }
    }

    public string CreatedAtJson
    {
        get => !TryGetData("createdAtJson", out string name) ? null : name;
        set
        {
            if (CreatedAtJson == value)
            {
                return;
            }

            SetData("createdAtJson", value);
        }
    }

    public IColShape ColShape { get; set; }
}