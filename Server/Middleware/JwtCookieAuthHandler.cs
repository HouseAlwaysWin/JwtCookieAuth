using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Server.Middleware
{

    public class JwtCookieAuthHandler : JwtBearerHandler<JwtBearerCookieOptions>
    {
        private readonly IRefreshTokenHandler _refreshTokenHandler;
        private readonly IOptionsMonitor<JwtBearerCookieOptions> _options;
        public JwtCookieAuthHandler(
            IRefreshTokenHandler refreshTokenHandler,
            IOptionsMonitor<JwtBearerCookieOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            this._refreshTokenHandler = refreshTokenHandler;
            this._options = options;
            if (this._refreshTokenHandler == null) throw new NullReferenceException("refreshTokenHandler is null");
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authResult = await base.HandleAuthenticateAsync();
            var jwtTokenName = _options.CurrentValue.JwtTokenName;
            var jwtRefreshTokenName = _options.CurrentValue.JwtRefreshTokenName;
            if (Request.Headers.ContainsKey(HeaderNames.Authorization) ||
                (!Request.Cookies.ContainsKey(jwtTokenName) && !Request.Cookies.ContainsKey(jwtRefreshTokenName)))
            {
                return authResult;
            }

            if (!authResult.Succeeded)
            {
                // TODO check refresh token
                await _refreshTokenHandler.HandleRefreshToken(Context);
            }

            Request.Headers[HeaderNames.Authorization] =
              $"Bearer {Request.Cookies[jwtTokenName]}";
            authResult = await base.HandleAuthenticateAsync();

            Request.Headers.Remove(HeaderNames.Authorization);
            return authResult;

        }
    }
}
