using JwtCookieAuth.Models;
using JwtCookieAuth.Providers.OAuth;
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
        string GenerateJwtToken(string tokenKey, string issuer, List<Claim> claims, int expiresTime = 5);
        RefreshToken GenerateRefreshToken(string ipAddress, Dictionary<string, string> claims, int expireTime = 10080);
        string GetAndSetAntiCsrfTokenCookie(HttpContext context);
        IOAuthProviderBase GetOAuthProviderInstance(string provider, string assemblyName = "");
        Task<OAuthUserInfoRes> GetOAuthUserInfoAsync(string code, string provider, HttpContext context, string assemblyName = "");
        void RemoveBearerTokenToCookie(HttpContext context);
        void RevokeRefreshToken(RefreshToken token, string ipAddress);
        string GetOAuthLoginUrl(string provider, string assemblyName = "");
        string GeneratePasswordHash(string password, string salt, int iterationCount = 10000, int numBytesRequested = 128);
        bool ValidatedPassword(string password, string passwordHash, string salt);
    }
}