using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Server.Models;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Middleware
{
    public class RefreshTokenHandler : IRefreshTokenHandler
    {
        private readonly ITokenService _tokenService;
        private readonly IOptionsMonitor<JwtBearerCookieOptions> _options;
        private readonly ICachedService _cachedService;

        public RefreshTokenHandler(
            ITokenService tokenService,
            IOptionsMonitor<JwtBearerCookieOptions> options,
            ICachedService cachedService)
        {
            this._tokenService = tokenService;
            this._options = options;
            this._cachedService = cachedService;
        }
        public async Task HandleRefreshToken(HttpContext context)
        {
            var refreshToken = context.Request.Cookies[_options.CurrentValue.JwtRefreshTokenName];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshTokens = await _cachedService.GetAndSetAsync("refreshTokens", new List<RefreshToken>());
                var jwtToken = context.Request.Cookies[_options.CurrentValue.JwtTokenName];
                var tokenCount = refreshTokens.Count(r => r.Token == refreshToken);
                if (tokenCount > 1)
                {
                    foreach (var rt in refreshTokens)
                    {
                        rt.Revoked = DateTime.Now;
                    }
                }

            }
        }
    }
}
