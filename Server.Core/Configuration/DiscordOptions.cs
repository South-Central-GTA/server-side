namespace Server.Core.Configuration;

public class DiscordOptions
{
    public string Token { get; set; }
    public bool IsLive { get; set; }
    public ulong WecomeMessageId { get; set; }
    public ulong VerifiedUserRoleId { get; set; }
    public ulong StaffUserRoleId { get; set; }
    public ulong TesterUserRoleId { get; set; }
    public ulong OwnerUserRoleId { get; set; }
    public ulong LeadAdminUserRoleId { get; set; }
    public ulong AdminUserRoleId { get; set; }
    public ulong ModUserRoleId { get; set; }
    public ulong DevUserRoleId { get; set; }
    public ulong FactionManagementFlagUserRoleId { get; set; }
    public ulong HeadOfFactionManagementFlagUserRoleId { get; set; }
    public ulong TeamManagementFlagUserRoleId { get; set; }
    public ulong HeadOfTeamManagementFlagUserRoleId { get; set; }
    public ulong EconomyManagementFlagUserRoleId { get; set; }
    public ulong HeadOfEconomyManagementFlagUserRoleId { get; set; }
    public ulong LoreAndEventManagementFlagUserRoleId { get; set; }
    public ulong HeadOfLoreAndEventManagementFlagUserRoleId { get; set; }
    public ulong FounderFlagUserRoleId { get; set; }
    public ulong ManageAnimationsFlagUserRoleId { get; set; }
}