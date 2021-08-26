using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OAuthController: ControllerBase
    {
        private readonly IConfiguration _config;

        public OAuthController(IConfiguration config)
        {
            this._config = config;
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
    }
}