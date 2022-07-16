using System;
using System.Collections.Generic;
using System.Numerics;
using AltV.Net.EntitySync;
using Server.Data.Enums.EntitySync;

namespace Server.Core.Entities;

public class ServerObject : Entity
{
    public ServerObject(Vector3 position, int dimension, uint range) : base((ulong)EntityType.OBJECT, position, dimension, range)
    {
    }

    public string Model
    {
        get => !TryGetData("model", out string model) ? null : model;
        set
        {
            if (Model == value)
            {
                return;
            }

            SetData("model", value);
        }
    }

    public string Name
    {
        get => !TryGetData("name", out string name) ? null : name;
        set
        {
            if (Model == value)
            {
                return;
            }

            SetData("name", value);
        }
    }

    public bool Freeze
    {
        get => TryGetData("freeze", out bool frozen) && frozen;
        set => SetData("freeze", value);
    }

    public bool OnFire
    {
        get => TryGetData("onFire", out bool onFire) && onFire;
        set => SetData("onFire", value);
    }

    public int ItemId
    {
        get => !TryGetData("itemId", out int id) ? -1 : id;
        set
        {
            if (ItemId == value)
            {
                return;
            }

            SetData("itemId", value);
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
}