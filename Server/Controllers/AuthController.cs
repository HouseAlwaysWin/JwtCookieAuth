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

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly Dictionary<string, OAuthProviderConfig> _oauthConfig;

        private readonly string ACCESSTOKEN_URL = "https://oauth2.googleapis.com/token";

        public AuthController(IConfiguration config,
        IOptions<Dictionary<string, OAuthProviderConfig>> oauthConfig)
        {
            this._config = config;
            this._oauthConfig = oauthConfig.Value;
        }


        [HttpGet("getOAuthUrl")]
        public ActionResult GetOAuthUrl(string providerName)
        {

            IOAuthProvider provider;
            string redirectUrl = string.Empty;
            if (providerName == "Google")
            {
                provider = new GoogleOAuthProvider(this._oauthConfig[providerName]);
                redirectUrl = provider.CreateLoginUrl();
            }

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                return BadRequest();
            }

            return Redirect(redirectUrl);
        }


        [HttpPost("getLineAccessToken")]
        public async Task<ActionResult> GetLineAccessToken([FromBody] LineTokenParam req)
        {
            HttpClient client = new HttpClient();
            var redirectUrl = _config["Line:redirect_url"];
            var clientId = _config["Line:client_id"];
            var clientSecret = _config["Line:client_secret"];
            var accessTokenUrl = _config["Line:accessTokenUrl"];

            var dict = new Dictionary<string, string>
            {
                { "grant_type","authorization_code" },
                { "redirect_uri",redirectUrl },
                { "client_id",clientId },
                { "client_secret",clientSecret },
                { "code",req.Code }
            };

            var values = new FormUrlEncodedContent(dict);
            var response = await client.PostAsync(accessTokenUrl, values);
            var resultString = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<LineTokenResponse>(resultString);

            return Ok(result);
        }

        [HttpPost("loginGoogle")]
        public async Task<ActionResult> loginGoogle([FromBody] LineTokenParam req)
        {
            HttpClient client = new HttpClient();
            var redirectUrl = _config["Google:RedirectUrl"];
            var clientId = _config["Google:AppId"];
            var clientSecret = _config["Google:AppSecret"];
            var accessTokenUrl = ACCESSTOKEN_URL;

            var dict = new Dictionary<string, string>
            {
                { "grant_type","authorization_code" },
                { "redirect_uri",redirectUrl },
                { "client_id",clientId },
                { "client_secret",clientSecret },
                { "code",req.Code }
            };

            var values = new FormUrlEncodedContent(dict);
            var response = await client.PostAsync(accessTokenUrl, values);
            var resultString = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<LineTokenResponse>(resultString);

            var handler = new JwtSecurityTokenHandler();
            var idToken = handler.ReadJwtToken(result.IdToken);
            var name = idToken.Claims.FirstOrDefault(c => c.Type == "name").Value;
            var email = idToken.Claims.FirstOrDefault(c => c.Type == "email").Value;
            string token = CreateToken(name, name, email);

            SetHttpOnlyCookie(token);

            return Ok(result);
        }

        private void SetHttpOnlyCookie(string token){
            var cookieOptions = new CookieOptions{
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true
            };
            Response.Cookies.Append("token",token,cookieOptions);
        }

        [HttpGet]
        public ActionResult IsAuth()
        {
            bool isAuth = User.Identity.IsAuthenticated;
            return Ok(isAuth);
        }

        private string CreateToken(string userId, string name, string email)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var tokenKey = (env == "Development") ? _config["Token:Key"] : Environment.GetEnvironmentVariable("Token:Key");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Email,email),
                new Claim(JwtRegisteredClaimNames.UniqueName,name),
                new Claim("Id",userId),
                new Claim("Name",name)
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var issuer = (env == "Development") ? _config["Token:Issuer"] : Environment.GetEnvironmentVariable("Token:Issuer");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
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