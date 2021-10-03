using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Server.Middleware
{
    public interface IRefreshTokenHandler
    {
        Task HandleRefreshToken(HttpContext context);
    }
}