namespace Server.Core.Configuration;

public class AccountOptions
{
    public int LoginTrysMax { get; set; }
    public int LoginTrysTimerMinutes { get; set; }
    public int MaxCharacters { get; set; }
    public int CharacterSlotPrice { get; set; }
    public int StartSouthCentralPoints { get; set; }
}