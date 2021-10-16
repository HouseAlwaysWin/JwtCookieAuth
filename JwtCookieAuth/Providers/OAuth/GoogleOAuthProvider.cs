using JwtCookieAuth.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Providers.OAuth
{
    public class GoogleOAuthProvider : OAuthProviderBase<OAuthConfigOptions>
    {
        public GoogleOAuthProvider(IOptionsMonitor<OAuthConfigOptions> options, IHttpClientFactory httpFactory) : base(options, httpFactory)
        {
        }

        public override async Task<OAuthUserInfoRes> GetOAuthUserInfoAsync(OAuthTokenRes tokenRes)
        {
            return await Task.Run(() =>
            {
                var handler = new JwtSecurityTokenHandler();
                var result = new OAuthUserInfoRes();
                if (!string.IsNullOrEmpty(tokenRes.IdToken))
                {
                    var info = handler.ReadJwtToken(tokenRes.IdToken);
                    if (info != null)
                    {
                        var name = info.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                        var email = info.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                        var pictureUrl = info.Claims?.FirstOrDefault(c => c.Type == "picture")?.Value;
                        result = new OAuthUserInfoRes
                        {
                            Email = email,
                            PictureUrl = pictureUrl,
                            Name = name,
                            Provider = OAuthProviderEnum.Google.ToString(),
                        };
                    }
                }

                return result;
            });
        }
    }

}
