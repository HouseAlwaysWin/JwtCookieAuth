using Server.Models;
using System.Threading.Tasks;

namespace Server.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> ExternalAuthenticate(ExternalAuthParam req);
        AuthResponse RefreshToken(string token, string ipAddress);
    }
}