using JwtCookieAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Providers.OAuth
{
    public abstract class OAuthProviderBase<TOptions> : IOAuthProviderBase where TOptions : OAuthConfigOptions
    {
        private readonly IOptionsMonitor<TOptions> _options;
        private readonly IHttpClientFactory _httpFactory;

        public OAuthProviderBase(IOptionsMonitor<TOptions> options,
            IHttpClientFactory httpFactory)
        {
            this._options = options;
            this._httpFactory = httpFactory;
        }

        /// <summary>
        /// 取得 Token 資訊
        /// </summary>
        /// <param name="code"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public virtual async Task<OAuthTokenRes> ExchangeCodeAsync(string code, OAuthProviderEnum provider, HttpContext context)
        {
            var httpClient = this._httpFactory.CreateClient();
            var option = _options.Get(Enum.GetName(provider));

            var tokenRequestParameters = new Dictionary<string, string>()
            {
                { "client_id", option.ClientId },
                { "redirect_uri", option.RedirectUrl },
                { "client_secret", option.ClientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
            };

            var requestContent = new FormUrlEncodedContent(tokenRequestParameters);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, option.TokenEndpoint);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = requestContent;
            var response = await httpClient.SendAsync(requestMessage, context.RequestAborted);
            if (response.IsSuccessStatusCode)
            {
                var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                return OAuthTokenRes.Success(payload);
            }
            else
            {
                var error = "OAuth token endpoint failure: " + await Display(response);
                return OAuthTokenRes.Failed(new Exception(error));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenRes"></param>
        /// <returns></returns>
        public virtual Task<OAuthUserInfoRes> GetOAuthUserInfoAsync(OAuthTokenRes tokenRes)
        {
            throw new NotImplementedException();
        }

        private static async Task<string> Display(HttpResponseMessage response)
        {
            var output = new StringBuilder();
            output.Append("Status: " + response.StatusCode + ";");
            output.Append("Headers: " + response.Headers.ToString() + ";");
            output.Append("Body: " + await response.Content.ReadAsStringAsync() + ";");
            return output.ToString();
        }

    }

}
