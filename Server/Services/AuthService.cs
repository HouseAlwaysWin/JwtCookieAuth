using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthService(
            ITokenService tokenService,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory)
        {
            this._tokenService = tokenService;
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this._httpClientFactory = httpClientFactory;
        }

        public async Task<AuthResponse> ExternalAuthenticate(ExternalAuthParam req)
        {

            var dict = new Dictionary<string, string>
            {
                { "grant_type","authorization_code" },
                { "redirect_uri",req.RedirectUrl },
                { "client_id",req.ClientId },
                { "client_secret",req.ClientSecret },
                { "code",req.Code }
            };

            var values = new FormUrlEncodedContent(dict);
            var response = await _httpClientFactory.CreateClient().PostAsync(req.TokenApiURI, values);
            var resultString = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<OAuthTokenResponse>(resultString);

            var tokenKey = _config["Token:Key"];
            var issuer = _config["Token:Issuer"];

            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var jwtToken = string.Empty;
            var refreshToken = new RefreshToken();
            var userInfo = new AuthResponse();

            switch (req.Provider)
            {
                case "Google":
                    var handler = new JwtSecurityTokenHandler();
                    var info = handler.ReadJwtToken(result.IdToken);
                    var name = info.Claims.FirstOrDefault(c => c.Type == "name").Value;
                    var email = info.Claims.FirstOrDefault(c => c.Type == "email").Value;
                    jwtToken = _tokenService.CreateJwtToken(tokenKey, issuer, new List<Claim>
                    {
                        new Claim("Provider","Google"),
                        new Claim("Name",name),
                        new Claim("Email",email)
                    });
                    refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

                    SetHttpOnlyCookie("jwtToken", jwtToken, 10);
                    SetHttpOnlyCookie("refreshToken", refreshToken.Token, 30);
                    userInfo.Email = email;
                    userInfo.Username = name;
                    userInfo.Provider = "Google";
                    break;
                case "Line":
                    break;
            }

            return userInfo;
        }


        private void SetHttpOnlyCookie(string name, string token, int expire)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTime.Now.AddMinutes(expire)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(name, token, cookieOptions);
        }

        public AuthResponse RefreshToken(string token, string ipAddress)
        {

            return null;
        }
    }
}
