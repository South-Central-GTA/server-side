using System;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Server.Core.Entities.Factories;

public class ServerVehicleFactory : IEntityFactory<IVehicle>
{
    public IVehicle Create(ICore core, IntPtr entityPointer, ushort id)
    {
        return new ServerVehicle(core, entityPointer, id);
    }
}