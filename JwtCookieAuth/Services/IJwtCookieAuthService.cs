using JwtCookieAuth.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtCookieAuth.Services
{
    public interface IJwtCookieAuthService
    {
        void AddBearerTokenToCookie(string token, string refreshToken, int tokenExpires, int refreshTokenExpires, HttpContext context);
        Task AddLoginCookiesAsync(List<Claim> claims, HttpContext context);
        Task ClaerRevokedRefreshToken();
        string GenerateCsrfToken(HttpContext httpContext);
        string GenerateJwtToken(string tokenKey, string issuer, List<Claim> claims, int expiresTime = 5);
        RefreshToken GenerateRefreshToken(string ipAddress, Dictionary<string, string> claims, int expireTime = 10080);
        string GetAndSetAntiCsrfTokenCookie(HttpContext context);
        Task<OAuthUserInfoRes> GetOAuthUserInfoAsync(string code, OAuthProviderEnum provider, HttpContext context, string assemblyName = "JwtCookieAuth.Providers.OAuth.OAuthProviders");
        void RemoveBearerTokenToCookie(HttpContext context);
        void RevokeRefreshToken(RefreshToken token, string ipAddress);
    }
}