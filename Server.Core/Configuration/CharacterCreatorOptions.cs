using System.Collections.Generic;

namespace Server.Core.Configuration;

public class CharacterCreatorOptions
{
    public List<string> WhitelistedClasses { get; set; }
    public List<string> BlacklistedModels { get; set; }
    public List<string> BlacklistedDlcs { get; set; }
    public int MaxSouthCentralPointsVehicles { get; set; }
    public int MaxSouthCentralPointsHouses { get; set; }
    public int CharacterBaseCosts { get; set; }
}