namespace Server.Data.Models;

public class PhoneCallData
{
    public PhoneCallData(ushort partnerPlayerId, string partnerPhoneNumber, bool startedCall)
    {
        PartnerPlayerId = partnerPlayerId;
        PartnerPhoneNumber = partnerPhoneNumber;
        IsInitiator = startedCall;
    }

    public int PartnerPlayerId { get; }
    public string PartnerPhoneNumber { get; }
    public bool IsInitiator { get; }
}