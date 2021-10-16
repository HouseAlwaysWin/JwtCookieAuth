using JwtCookieAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Providers.OAuth
{
    public class LineOAuthProvider : OAuthProviderBase<OAuthConfigOptions>
    {
        private readonly IOptionsMonitor<OAuthConfigOptions> _options;
        private readonly IHttpClientFactory _httpFactory;

        public LineOAuthProvider(IOptionsMonitor<OAuthConfigOptions> options,
            IHttpClientFactory httpFactory) : base(options, httpFactory)
        {
            this._options = options;
            this._httpFactory = httpFactory;
        }
        public override async Task<OAuthUserInfoRes> GetOAuthUserInfoAsync(OAuthTokenRes tokenRes)
        {
            var result = new OAuthUserInfoRes();
            var handler = new JwtSecurityTokenHandler();

            if (!string.IsNullOrEmpty(tokenRes.IdToken))
            {
                var info = handler.ReadJwtToken(tokenRes.IdToken);
                if (info != null)
                {
                    var name = info.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                    var email = info.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                    var pictureUrl = info.Claims?.FirstOrDefault(c => c.Type == "picture")?.Value;
                    result.Email = email;
                    result.Name = name;
                    result.PictureUrl = pictureUrl;
                }
            }

            var userEndpoint = this._options.Get(OAuthProviderEnum.Line.ToString())?.UserInformationEndpoint;
            if (!string.IsNullOrEmpty(userEndpoint))
            {
                var httpClient = this._httpFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenRes.AccessToken}");
                var profileRes = await httpClient.GetAsync(userEndpoint);
                if (profileRes.IsSuccessStatusCode)
                {
                    var payload = JObject.Parse(await profileRes.Content.ReadAsStringAsync());
                    result.Id = payload.Value<string>("userId");
                    result.Name = payload.Value<string>("displayName");
                    result.PictureUrl = payload.Value<string>("pictureUrl");
                }
            }

            result.Provider = OAuthProviderEnum.Line.ToString();

            return result;
        }
    }

}
