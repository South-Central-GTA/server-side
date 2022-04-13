using System.Threading.Tasks;

namespace Server.Core.Abstractions;

public interface IServerJob
{
    Task OnStartup();

    Task OnSave();

    Task OnShutdown();
}