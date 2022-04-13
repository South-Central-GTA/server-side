namespace Server.Core.Configuration;

public class DevelopmentOptions
{
    public bool DebugUi { get; set; }
    public string CefIp4 { get; set; }
    public bool LocalDb { get; set; }
    public bool SeedingDefaultDataIntoDatabase { get; set; }
    public bool DropDatabaseAtStartup { get; set; }
}