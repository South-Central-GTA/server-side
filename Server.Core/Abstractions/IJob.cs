using System.Threading.Tasks;

namespace Server.Core.Abstractions;

public interface IJob
{
    Task OnStartup();

    Task OnSave();

    Task OnShutdown();
}