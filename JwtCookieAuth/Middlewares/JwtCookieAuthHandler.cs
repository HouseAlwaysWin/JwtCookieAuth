using JwtCookieAuth.Extensions;
using JwtCookieAuth.Models;
using JwtCookieAuth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace JwtCookieAuth.Middlewares
{
    public class JwtCookieAuthHandler : JwtBearerHandler<JwtCookieBearerOptions>
    {
        private readonly IJwtCookieAuthService _authService;
        private readonly ICachedService _cachedService;
        private readonly JwtCookieBearerOptions _cookieOptions;
        private readonly ILogger<JwtCookieAuthHandler> _logger;


        public JwtCookieAuthHandler(
            IJwtCookieAuthService authService,
            ICachedService cachedService,
            IOptionsMonitor<JwtCookieBearerOptions> options,
            ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            this._authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this._cachedService = cachedService ?? throw new ArgumentNullException(nameof(authService)); ;
            this._cookieOptions = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            this._logger = logger.CreateLogger<JwtCookieAuthHandler>() ?? throw new ArgumentNullException(nameof(logger));
        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authResult = await base.HandleAuthenticateAsync();
            var jwtTokenName = _cookieOptions.JwtName;
            var refreshTokenName = _cookieOptions.RefreshTokenName;

            if (Request.Headers.ContainsKey(HeaderNames.Authorization) ||
                (!Request.Cookies.ContainsKey(jwtTokenName) && !Request.Cookies.ContainsKey(refreshTokenName)))
            {
                return authResult;
            }

            Request.Headers[HeaderNames.Authorization] =
              $"Bearer {Request.Cookies[jwtTokenName]}";
            try
            {
                authResult = await base.HandleAuthenticateAsync();
            }
            catch
            {
                throw;
            }


            if (!authResult.Succeeded)
            {
                _logger.LogInformation(authResult?.Failure?.Message);
                var cookieRefreshToken = Request.Cookies[_cookieOptions.RefreshTokenName];
                var newJwtToken = string.Empty;
                if (cookieRefreshToken != null)
                {
                    string ipAddress = Context.Connection.RemoteIpAddress.ToString();
                    var refreshTokenAll = await _cachedService.GetAsync<List<RefreshToken>>(_cookieOptions.RefreshTokensAllCachedName);

                    if (refreshTokenAll != null)
                    {
                        var refreshTokens = refreshTokenAll.Where(r => r.Token == cookieRefreshToken).ToList();
                        if (refreshTokens.Count > 2)
                        {
                            _authService.RemoveBearerTokenToCookie(Context);
                            foreach (var rf in refreshTokens)
                            {
                                _authService.RevokeRefreshToken(rf, ipAddress);
                            }
                            return authResult;
                        }

                        if (refreshTokens.Count == 0)
                        {
                            return authResult;
                        }

                        var oldRefreshToken = refreshTokens.FirstOrDefault();
                        if (oldRefreshToken.IsRevoked || oldRefreshToken.IsExpired)
                        {
                            return authResult;
                        };

                        var userClaims = oldRefreshToken.UserClaims.ConvertToClaims();

                        newJwtToken = _authService.GenerateJwtToken(_cookieOptions.JwtKey, _cookieOptions.JwtIssuer, userClaims, _cookieOptions.JwtExpires);
                        var newRefreshToken = _authService.GenerateRefreshToken(ipAddress, oldRefreshToken.UserClaims, _cookieOptions.RefreshTokenExpires);

                        _authService.RevokeRefreshToken(oldRefreshToken, ipAddress);
                        refreshTokenAll.Add(newRefreshToken);
                        await _cachedService.SetAsync(_cookieOptions.RefreshTokensAllCachedName, refreshTokenAll);

                        _authService.AddBearerTokenToCookie(newJwtToken, newRefreshToken.Token, _cookieOptions.JwtExpires, _cookieOptions.RefreshTokenExpires, Context);

                    }
                }

                if (!string.IsNullOrEmpty(newJwtToken))
                {
                    Request.Headers[HeaderNames.Authorization] =
                                   $"Bearer {newJwtToken}";
                    authResult = await base.HandleAuthenticateAsync();
                }
            }

            Request.Headers.Remove(HeaderNames.Authorization);
            return authResult;
        }
    }

}
