using System;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Server.Core.Entities.Factories;

public class ServerPlayerFactory
    : IEntityFactory<IPlayer>
{
    public IPlayer Create(IServer server, IntPtr entityPointer, ushort id)
    {
        return new ServerPlayer(server, entityPointer, id);
    }
}