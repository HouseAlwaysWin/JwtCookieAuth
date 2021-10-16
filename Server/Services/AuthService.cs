using AutoMapper;
using JwtCookieAuth.Extensions;
using JwtCookieAuth.Models;
using JwtCookieAuth.Services;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtCookieAuthService _jwtAuthService;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;

        public AuthService(
            IJwtCookieAuthService jwtAuthService,
            IMapper mapper,
            ILogger<AuthService> logger
            )
        {
            this._jwtAuthService = jwtAuthService;
            this._logger = logger;
            this._mapper = mapper;
        }

        /// <summary>
        /// 第三方登入
        /// </summary>
        /// <param name="code"></param>
        /// <param name="provider">登入參數</param>
        /// <returns></returns>
        public async Task<ExternalAuthResponse> ExternalLogin(string code, string provider, HttpContext context)
        {

            OAuthUserInfoRes userInfo = null;
            try
            {
                userInfo = await _jwtAuthService.GetOAuthUserInfoAsync(code, provider, context);
                var claims = userInfo.ConvertToClaims();
                await _jwtAuthService.AddLoginCookiesAsync(claims, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            var authRes = _mapper.Map<OAuthUserInfoRes, ExternalAuthResponse>(userInfo);
            return authRes;
        }


        /// <summary>
        /// 登出
        /// </summary>
        public void Logout(HttpContext context)
        {
            _jwtAuthService.RemoveBearerTokenToCookie(context);
        }

        /// <summary>
        /// 取得Csrf Token
        /// </summary>
        /// <returns></returns>
        public string GetAntiCsrfToken(HttpContext context)
        {
            return _jwtAuthService.GetAndSetAntiCsrfTokenCookie(context);
        }

        public string GetOAuthLoginUrl(string provider)
        {
            var oauthProvider = _jwtAuthService.GetOAuthProviderInstance(provider);
            string url = oauthProvider.GetOAuthLoginUrl(provider);
            return url;
        }

    }
}
