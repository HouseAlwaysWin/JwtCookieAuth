using JwtCookieAuth.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace JwtCookieAuth.Providers.OAuth
{
    public interface IOAuthProviderBase
    {
        Task<OAuthTokenRes> ExchangeCodeAsync(string code, string provider, HttpContext context);
        Task<OAuthUserInfoRes> GetOAuthUserInfoAsync(OAuthTokenRes tokenRes);
        string GetOAuthLoginUrl(string provider);
    }
}