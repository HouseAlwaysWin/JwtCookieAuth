using JwtCookieAuth.Extensions;
using JwtCookieAuth.Models;
using JwtCookieAuth.Providers.OAuth;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Services
{
    public class JwtCookieAuthService : IJwtCookieAuthService
    {
        private readonly IAntiforgery _antiforgery;
        private readonly IOptionsMonitor<OAuthConfigOptions> _oauthOptions;
        private readonly IHttpClientFactory _httpFactory;
        private readonly AntiforgeryOptions _antiforgeryOptions;
        private readonly ICachedService _cachedService;
        private readonly JwtCookieBearerOptions _jwtOptions;

        public JwtCookieAuthService(
            IAntiforgery antiforgery,
            IOptionsMonitor<AntiforgeryOptions> antiforgeryOptions,
            IOptionsMonitor<JwtCookieBearerOptions> jwtOptions,
            IOptionsMonitor<OAuthConfigOptions> oauthOptions,
            IHttpClientFactory httpFactory,
            ICachedService cachedService)
        {
            this._antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
            this._oauthOptions = oauthOptions ?? throw new ArgumentNullException(nameof(oauthOptions));
            this._httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
            this._antiforgeryOptions = antiforgeryOptions.CurrentValue ?? throw new ArgumentNullException(nameof(antiforgeryOptions));
            this._cachedService = cachedService ?? throw new ArgumentNullException(nameof(cachedService));
            this._jwtOptions = jwtOptions.CurrentValue ?? throw new ArgumentNullException(nameof(jwtOptions));
        }
        /// <summary>
        /// 產生Jwt Token
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="issuer"></param>
        /// <param name="claims"></param>
        /// <param name="expiresTime">過期時間(分鐘)</param>
        /// <returns></returns>
        public string GenerateJwtToken(string tokenKey, string issuer, List<Claim> claims, int expiresTime = 5)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresTime),
                SigningCredentials = creds,
                Issuer = issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenstring = tokenHandler.WriteToken(token);
            return tokenstring;

        }


        /// <summary>
        /// 產生Refresh Token
        /// </summary>
        /// <param name="ipAddress">Ip位址</param>
        /// <param name="claims">使用者Claims資訊</param>
        /// <param name="expireTime">過期時間(預設為七天10080分鐘)</param>
        /// <returns></returns>
        public RefreshToken GenerateRefreshToken(string ipAddress, Dictionary<string, string> claims, int expireTime = 10080)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddMinutes(expireTime),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserClaims = claims
            };

            return refreshToken;
        }

        public void RevokeRefreshToken(RefreshToken token, string ipAddress)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        ///// <summary>
        ///// 產生Csrf Token
        ///// </summary>
        ///// <param name="httpContext"></param>
        ///// <returns></returns>
        //public string GenerateCsrfToken(HttpContext httpContext)
        //{
        //    var res = this._antiforgery.GetAndStoreTokens(httpContext);
        //    if (!httpContext.Request.Cookies.ContainsKey(_antiforgeryOptions.Cookie.Name))
        //    {
        //        httpContext.Response.Cookies.Append(_antiforgeryOptions.Cookie.Name, res.CookieToken, new CookieOptions
        //        {
        //            SameSite = SameSiteMode.None,
        //            Secure = true
        //        });
        //    }

        //    return res.RequestToken;
        //}

        /// <summary>
        ///  新增Jwt Token和RefreshToken到Http only Cookie
        /// </summary>
        /// <param name="token"></param>
        /// <param name="refreshToken"></param>
        /// <param name="tokenExpires"></param>
        /// <param name="refreshTokenExpires"></param>
        public void AddBearerTokenToCookie(string token, string refreshToken, int tokenExpires, int refreshTokenExpires, HttpContext context)
        {
            SetHttpOnlyCookie(_jwtOptions.JwtName, token, tokenExpires, context);
            SetHttpOnlyCookie(_jwtOptions.RefreshTokenName, refreshToken, refreshTokenExpires, context);
        }

        /// <summary>
        ///  移除Jwt Token和RefreshToken到Http only Cookie
        /// </summary>
        public void RemoveBearerTokenToCookie(HttpContext context)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
            };
            context.Response.Cookies.Delete(_jwtOptions.JwtName, cookieOptions);
            context.Response.Cookies.Delete(_jwtOptions.RefreshTokenName, cookieOptions);
        }

        /// <summary>
        /// 設定HttpOnlyCookie
        /// </summary>
        /// <param name="name">Cookie名稱</param>
        /// <param name="token">Token</param>
        /// <param name="expiresTimes">過期時間</param>
        private void SetHttpOnlyCookie(string name, string token, int expiresTimes, HttpContext context)
        {

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Secure = true,
                Expires = DateTime.Now.AddMinutes(expiresTimes)
            };
            context.Response.Cookies.Append(name, token, cookieOptions);
        }


        /// <summary>
        /// 產生登入Jwt和Refresh Token
        /// </summary>
        /// <param name="claims"></param>
        public async Task AddLoginCookiesAsync(List<Claim> claims, HttpContext context)
        {
            string token = GenerateJwtToken(_jwtOptions.JwtKey, _jwtOptions.JwtIssuer, claims, _jwtOptions.JwtExpires);
            string ipAddress = context.Connection.RemoteIpAddress.ToString();
            var refreshToken = GenerateRefreshToken(ipAddress, claims.ConvertToDictionary());
            AddBearerTokenToCookie(token, refreshToken.Token, _jwtOptions.JwtExpires, _jwtOptions.RefreshTokenExpires, context);
            var refreshTokens = await _cachedService.GetAndSetAsync(_jwtOptions.RefreshTokensAllCachedName, new List<RefreshToken>());
            if (refreshTokens != null)
            {
                refreshTokens.Add(refreshToken);
                await _cachedService.SetAsync(_jwtOptions.RefreshTokensAllCachedName, refreshTokens);
            }
        }

        public async Task ClaerRevokedRefreshToken()
        {
            var refreshTokens = await _cachedService.GetAsync<List<RefreshToken>>(_jwtOptions.RefreshTokensAllCachedName);
            if (refreshTokens != null)
            {
                refreshTokens.RemoveAll(r => r.IsRevoked);
            }
        }


        public string GetAndSetAntiCsrfTokenCookie(HttpContext context)
        {
            var res = this._antiforgery.GetAndStoreTokens(context);
            //this._antiforgery.SetCookieTokenAndHeader(context);

            if (!context.Request.Cookies.ContainsKey(_antiforgeryOptions.Cookie.Name))
            {
                context.Response.Cookies.Append(_antiforgeryOptions.Cookie.Name, res.CookieToken, new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    Secure = true
                });
            }
            return res.RequestToken;
        }

        private IOAuthProviderBase GetOAuthProviderInstance(string assemblyname, OAuthProviderEnum provider, HttpContext context)
        {
            try
            {
                Assembly currentAssem = Assembly.GetExecutingAssembly();
                var type = currentAssem.GetType($"{assemblyname}.{provider}OAuthProvider");
                if (type == null) throw new NullReferenceException($"type of {provider}OAuthProvider not found,please implement {provider}OAuthProvider.");
                var linePro = new LineOAuthProvider(_oauthOptions, _httpFactory);
                var oauthHandler = (IOAuthProviderBase)Activator.CreateInstance(type, this._oauthOptions, _httpFactory);
                return oauthHandler;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 取得OAuth 會員資訊
        /// </summary>
        /// <param name="code">存取碼</param>
        /// <param name="assemblyname">OAuth Provider的namespace </param>
        /// <param name="provider">OAuth 提供者</param>
        /// <returns></returns>
        public async Task<OAuthUserInfoRes> GetOAuthUserInfoAsync(string code, OAuthProviderEnum provider, HttpContext context, string assemblyName = "JwtCookieAuth.Providers.OAuth")
        {
            var oauthProvider = GetOAuthProviderInstance(assemblyName, provider, context);
            OAuthTokenRes oauthResult = await oauthProvider.ExchangeCodeAsync(code, provider, context);
            var userInfo = await oauthProvider.GetOAuthUserInfoAsync(oauthResult);
            return userInfo;
        }




    }
}
