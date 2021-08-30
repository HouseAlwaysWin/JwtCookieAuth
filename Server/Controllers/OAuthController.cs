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

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly Dictionary<string, OAuthProviderConfig> _oauthConfig;

        private readonly string GOOGLE_ACCESSTOKEN_URL = "https://oauth2.googleapis.com/token";

        public OAuthController(IConfiguration config,
        IOptions<Dictionary<string, OAuthProviderConfig>> oauthConfig)
        {
            this._config = config;
            this._oauthConfig = oauthConfig.Value;
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(OAuthLoginParam req)
        {

            HttpClient client = new HttpClient();
            var redirectUrl = _config[$"{req.Provider}:RedirectUrl"];
            var clientId = _config[$"{req.Provider}:ClientId"];
            var clientSecret = _config[$"{req.Provider}:ClientSecret"];
            var accessTokenUrl = _config[$"{req.Provider}:AccessTokenUrl"];

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
            var result = JsonConvert.DeserializeObject<OAuthTokenResponse>(resultString);


            var token = string.Empty;

            switch (req.Provider)
            {
                case "Google":
                    token = GenerateGoogleToken(result.IdToken);
                    break;
                case "Line":
                    token = await GenerateLineTokenAsync(result.AccessToken);
                    break;
                default:
                    return BadRequest();
            }

            return Ok(new { token });
        }

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



        //[HttpPost("loginLine")]
        //public async Task<ActionResult> GetLineAccessToken([FromBody] LineTokenParam req)
        //{
        //    HttpClient client = new HttpClient();
        //    var redirectUrl = _config["Line:RedirectUrl"];
        //    var clientId = _config["Line:ClientId"];
        //    var clientSecret = _config["Line:ClientSecret"];
        //    var accessTokenUrl = _config["Line:AccessTokenUrl"];

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

        //    return Ok(result);
        //}

        //[HttpPost("loginGoogle")]
        //public async Task<ActionResult> loginGoogle([FromBody] LineTokenParam req)
        //{
        //    HttpClient client = new HttpClient();
        //    var redirectUrl = _config["Google:RedirectUrl"];
        //    var clientId = _config["Google:AppId"];
        //    var clientSecret = _config["Google:AppSecret"];
        //    var accessTokenUrl = GOOGLE_ACCESSTOKEN_URL;

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

        //    var handler = new JwtSecurityTokenHandler();
        //    var idToken = handler.ReadJwtToken(result.IdToken);
        //    var name = idToken.Claims.FirstOrDefault(c => c.Type == "name").Value;
        //    var email = idToken.Claims.FirstOrDefault(c => c.Type == "email").Value;
        //    string token = CreateToken(name, name, email);

        //    return Ok(new { token });
        //}

        [HttpGet("isAuth")]
        public ActionResult IsAuth()
        {
            bool isAuth = User.Identity.IsAuthenticated;
            return Ok(isAuth);
        }

        private string CreateToken(List<Claim> claims)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var tokenKey = (env == "Development") ? _config["Token:Key"] : Environment.GetEnvironmentVariable("Token:Key");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            //var claims = new List<Claim>{
            //    new Claim(JwtRegisteredClaimNames.Email,email),
            //    new Claim(JwtRegisteredClaimNames.UniqueName,name),
            //    new Claim("Provider",provider),
            //    new Claim("Name",name),
            //    new Claim("Email",email)

            //};

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