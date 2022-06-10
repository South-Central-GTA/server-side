using System.Numerics;
using AltV.Net.EntitySync;
using Server.Data.Enums.EntitySync;
using Server.Database.Models.Character;

namespace Server.Core.Entities;

public class ServerPed : Entity
{
    public ServerPed(Vector3 position, int dimension, uint range) : base((ulong)EntityType.PED, position, dimension,
        range)
    {
    }

    public string Model
    {
        get => !TryGetData("model", out string name) ? null : name;
        set
        {
            if (Model == value)
            {
                return;
            }

            SetData("model", value);
        }
    }

    public float Heading
    {
        get => !TryGetData("heading", out float heading) ? 0 : heading;
        set => SetData("heading", value);
    }

    public ServerVehicle? Vehicle
    {
        get => !TryGetData("vehicle", out ServerVehicle vehicle) ? null : vehicle;
        set => SetData("vehicle", value);
    }

    public float? Seat
    {
        get => !TryGetData("seat", out int seat) ? null : seat;
        set => SetData("seat", value);
    }

    public CharacterModel? CharacterModel
    {
        get => !TryGetData("characterModel", out CharacterModel value) ? null : value;
        set => SetData("characterModel", value);
    }
}