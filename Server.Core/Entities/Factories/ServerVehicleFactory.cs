using System;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Server.Core.Entities.Factories;

public class ServerVehicleFactory
    : IEntityFactory<IVehicle>
{
    public IVehicle Create(IServer server, IntPtr entityPointer, ushort id)
    {
        return new ServerVehicle(server, entityPointer, id);
    }
}