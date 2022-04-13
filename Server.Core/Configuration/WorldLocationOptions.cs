using System.Collections.Generic;
using Server.Data.Models;

namespace Server.Core.Configuration;

public class WorldLocationOptions
{
    public float LoginPositionX { get; set; }
    public float LoginPositionY { get; set; }
    public float LoginPositionZ { get; set; }

    public float CharacterSelectionPositionX { get; set; }
    public float CharacterSelectionPositionY { get; set; }
    public float CharacterSelectionPositionZ { get; set; }

    public float HarbourSelectionPositionX { get; set; }
    public float HarbourSelectionPositionY { get; set; }
    public float HarbourSelectionPositionZ { get; set; }

    public float RespawnPositionX { get; set; }
    public float RespawnPositionY { get; set; }
    public float RespawnPositionZ { get; set; }

    public InteriorData[] IntPositions { get; set; }
    public List<PublicGarageData> PublicGarages { get; set; }
    public List<DrivingSchoolData> DrivingSchools { get; set; }
}