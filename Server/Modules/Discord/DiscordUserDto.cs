using System.Text.Json.Serialization;

namespace Server.Modules.Discord;

public struct DiscordUserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Discriminator { get; set; }
    public string Avatar { get; set; }
    public bool Verified { get; set; }
    public string Email { get; set; }
    public int Flags { get; set; }
    public string Banner { get; set; }
    public int AccentColor { get; set; }
    public int PremiumType { get; set; }
    public int PublicFlags { get; set; }
}