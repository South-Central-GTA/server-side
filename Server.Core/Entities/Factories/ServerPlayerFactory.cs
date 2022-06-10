using System;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Server.Core.Entities.Factories;

public class ServerPlayerFactory : IEntityFactory<IPlayer>
{
    public IPlayer Create(ICore core, IntPtr entityPointer, ushort id)
    {
        return new ServerPlayer(core, entityPointer, id);
    }
}