using System.Collections.Generic;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Data.Dumps;

namespace Server.Modules.Dump;

public class VehicleDumpModule : ISingletonScript
{
    public List<VehicleDumpEntry> Dump { get; private set; }

    public void SetData(List<VehicleDumpEntry> dump)
    {
        Dump = dump;
    }
}