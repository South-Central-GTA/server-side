namespace Server.Core.Abstractions.ScriptStrategy;

/// <summary>
/// Services are created each time they are requested. It gets a new instance of the injected object, on each request of this object.
/// </summary>
public interface ITransientScript : IBaseScript
{
}