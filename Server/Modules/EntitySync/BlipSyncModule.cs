using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.EntitySync;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Data.Enums.EntitySync;
using BlipType = Server.Data.Enums.BlipType;

namespace Server.Modules.EntitySync;

public class BlipSyncModule : ISingletonScript
{
    private readonly Dictionary<ulong, ServerBlip> _blips = new();

    private readonly CompanyOptions _companyOptions;
    
    public BlipSyncModule(IOptions<CompanyOptions> companyOptions)
    {
        _companyOptions = companyOptions.Value;
    }
    
    public ServerBlip Create(string name, int color, float scale, bool shortRange, int spriteId, Position position, 
                             int dimension = 0, BlipType blipType = BlipType.POINT, ServerPlayer? targetPlayer = null, 
                             int radius = 0, int alpha = 255, uint streamRange = 5000)
    {
        var blip = new ServerBlip(position, dimension, streamRange)
        {
            BlipType = blipType,
            Player = targetPlayer,
            Color = color,
            Scale = scale,
            ShortRange = shortRange,
            Sprite = spriteId,
            Name = name,
            Radius = radius,
            Alpha = alpha
        };

        AltEntitySync.AddEntity(blip);
        _blips.Add(blip.Id, blip);

        return blip;
    }

    public bool Delete(ulong blipId)
    {
        var serverBlip = Get(blipId);
        if (serverBlip == null)
        {
            return false;
        }

        AltEntitySync.RemoveEntity(serverBlip);
        _blips.Remove(blipId);

        return true;
    }

    public void DeleteAll()
    {
        foreach (var serverBlip in GetAll())
        {
            AltEntitySync.RemoveEntity(serverBlip);
        }
        
        _blips.Clear();
    }

    public ServerBlip? Get(ulong objectId)
    {
        if (!AltEntitySync.TryGetEntity(objectId, (ulong)EntityType.BLIP, out var entity))
        {
            return null;
        }

        return entity is not ServerBlip serverBlip ? default : serverBlip;
    }
    
    public List<ServerBlip> GetAll()
    {
        return _blips.Select(entity => entity.Value).ToList();
    }
}