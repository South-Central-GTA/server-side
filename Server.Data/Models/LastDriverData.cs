using System;

namespace Server.Data.Models;

/// <summary>
///     We are using this class as a json string in our database vehicle.
/// </summary>
public class LastDriverData
{
    public LastDriverData(int characterId)
    {
        CharacterId = characterId;
        Date = DateTime.Now;
    }

    public int CharacterId { get; set; }
    public DateTime Date { get; set; }
}