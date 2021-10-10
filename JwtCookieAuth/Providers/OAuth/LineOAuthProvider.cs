using JwtCookieAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            var httpClient = this._httpFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenRes.AccessToken}");
            var userEndpoint = this._options.Get(OAuthProviderEnum.Line.ToString()).UserInformationEndpoint;
            var profileRequest = await httpClient.GetAsync(userEndpoint);
            var profileString = await profileRequest.Content.ReadAsStringAsync();
            LineProfileRes profileRes = JsonConvert.DeserializeObject<LineProfileRes>(profileString);

            var result = new OAuthUserInfoRes
            {
                Id = profileRes.UserId,
                PictureUrl = profileRes.PictureUrl,
                Name = profileRes.DisplayName,
                Provider = OAuthProviderEnum.Line
            };
            return result;
        }
    }

}
