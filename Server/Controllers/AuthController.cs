using System.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Server.Models;
using Server.OauthPrividers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Server.Services;
using Microsoft.Extensions.Logging;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly IAntiforgery _antiforgery;
        private readonly Dictionary<string, OAuthProviderConfig> _oauthConfig;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        private readonly string GOOGLE_ACCESSTOKEN_URL = "https://oauth2.googleapis.com/token";

        public AuthController(
            IConfiguration config,
            ITokenService tokenService,
            IAntiforgery antiforgery,
            IAuthService authService,
            IOptions<Dictionary<string, OAuthProviderConfig>> oauthConfig,
            ILogger<AuthController> logger)
        {
            this._config = config;
            this._tokenService = tokenService;
            this._antiforgery = antiforgery;
            this._oauthConfig = oauthConfig.Value;
            this._authService = authService;
            this._logger = logger;
        }


        [HttpGet("getCsrfToken")]
        public ActionResult GetCsrfToken()
        {
            var token = this._tokenService.GenerateCsrfToken(HttpContext);
            return Ok(new { token = token });
        }


        [Authorize]
        [HttpGet("logout")]
        public ActionResult Logout()
        {
            //var cookieOptions = new CookieOptions
            //{
            //    HttpOnly = true,
            //    SameSite = SameSiteMode.None,
            //    Secure = true,
            //};
            Response.Cookies.Delete("jwtToken");
            Response.Cookies.Delete("refreshToken");
            return Ok();
        }


        [HttpPost("ExternalLogin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLogin(ExternalLoginReq req)
        {
            try
            {
                var redirectUrl = _config[$"{req.Provider}:RedirectUrl"];
                var clientId = _config[$"{req.Provider}:ClientId"];
                var clientSecret = _config[$"{req.Provider}:ClientSecret"];
                var accessTokenUrl = _config[$"{req.Provider}:AccessTokenUrl"];

                var userInfo = await _authService.ExternalAuthenticate(new ExternalAuthParam
                {
                    RedirectUrl = redirectUrl,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    TokenApiURI = accessTokenUrl,
                    Code = req.Code,
                    Provider = req.Provider
                });
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return BadRequest();

        }


        //[HttpPost("login")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(OAuthLoginParam req)
        //{

        //    HttpClient client = new HttpClient();
        //    var redirectUrl = _config[$"{req.Provider}:RedirectUrl"];
        //    var clientId = _config[$"{req.Provider}:ClientId"];
        //    var clientSecret = _config[$"{req.Provider}:ClientSecret"];
        //    var accessTokenUrl = _config[$"{req.Provider}:AccessTokenUrl"];

        //    var dict = new Dictionary<string, string>
        //    {
        //        { "grant_type","authorization_code" },
        //        { "redirect_uri",redirectUrl },
        //        { "client_id",clientId },
        //        { "client_secret",clientSecret },
        //        { "code",req.Code }
        //    };

        //    var values = new FormUrlEncodedContent(dict);
        //    var response = await client.PostAsync(accessTokenUrl, values);
        //    var resultString = response.Content.ReadAsStringAsync().Result;
        //    var result = JsonConvert.DeserializeObject<OAuthTokenResponse>(resultString);


        //    var token = string.Empty;

        //    switch (req.Provider)
        //    {
        //        case "Google":
        //            token = GenerateGoogleToken(result.IdToken);
        //            break;
        //        case "Line":
        //            token = await GenerateLineTokenAsync(result.AccessToken);
        //            break;
        //        default:
        //            return BadRequest();
        //    }

        //    SetHttpOnlyCookie(token);

        //    return Ok();
        //}

        private async Task<string> GenerateLineTokenAsync(string token)
        {
            var profileUrl = _config["Line:AccessProfileUrl"];

            //var handler = new JwtSecurityTokenHandler();
            //var info = handler.ReadJwtToken(idToken);
            //var name = info.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            //var email = info.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var profileRequest = await httpClient.GetAsync(profileUrl);
            var profileString = await profileRequest.Content.ReadAsStringAsync();
            LineProfileResponse profileRes = JsonConvert.DeserializeObject<LineProfileResponse>(profileString);



            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.NameId,profileRes.UserId),
                new Claim(JwtRegisteredClaimNames.UniqueName,profileRes.DisplayName),
                new Claim("Provider","Line"),
                new Claim("Name",profileRes.DisplayName),
            };

            string newToken = CreateToken(claims);

            return newToken;
        }

        private string GenerateGoogleToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var info = handler.ReadJwtToken(idToken);
            var name = info.Claims.FirstOrDefault(c => c.Type == "name").Value;
            var email = info.Claims.FirstOrDefault(c => c.Type == "email").Value;

            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Email,email),
                new Claim(JwtRegisteredClaimNames.UniqueName,name),
                new Claim("Provider","Line"),
                new Claim("Name",name),
                new Claim("Email",email)

            };
            string token = CreateToken(claims);

            return token;
        }


        [HttpGet("isAuth")]
        public ActionResult IsAuth()
        {
            bool isAuth = User.Identity.IsAuthenticated;
            return Ok(isAuth);
        }


        private void SetHttpOnlyCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            };
            Response.Cookies.Append("token", token, cookieOptions);
        }

        private string CreateToken(List<Claim> claims)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var tokenKey = (env == "Development") ? _config["Token:Key"] : Environment.GetEnvironmentVariable("Token:Key");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var issuer = (env == "Development") ? _config["Token:Issuer"] : Environment.GetEnvironmentVariable("Token:Issuer");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(1),
                SigningCredentials = creds,
                Issuer = issuer
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);


            var tokenstring = tokenHandler.WriteToken(token);
            return tokenstring;
        }


    }
}