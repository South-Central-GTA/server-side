using System.Threading.Tasks;
using Refit;

namespace Server.Modules.Discord;

public interface IDiscordApi
{
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    [Get("/users/@me")]
    Task<ApiResponse<string>> GetUser([Authorize()] string authorization);
}