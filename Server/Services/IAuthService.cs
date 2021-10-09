using Microsoft.AspNetCore.Http;
using Server.Models;
using System.Threading.Tasks;

namespace Server.Services
{
    public interface IAuthService
    {
        Task<ExternalAuthResponse> ExternalLogin(string code, string provider, HttpContext context);
        string GetAntiCsrfToken(HttpContext context);
        void Logout(HttpContext context);
    }
}