namespace Server.Core.Abstractions.ScriptStrategy;

/// <summary>
/// Services are created once for the lifetime of the application. It uses the same instance for the whole application.
/// </summary>
public interface ISingletonScript : IBaseScript
{
}