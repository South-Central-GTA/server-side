namespace Server.Core.Abstractions.ScriptStrategy;

/// <summary>
/// Services are created on each request (once per request).
/// </summary>
public interface IScopedScript : IBaseScript
{
    
}