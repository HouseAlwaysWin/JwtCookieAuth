using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Models
{
    public class OAuthTokenRes
    {
        public OAuthTokenRes(string accessToken, string tokenType, string refreshToken, string expiresIn)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }

        private OAuthTokenRes(JObject response)
        {
            Response = response;
            AccessToken = response.Value<string>("access_token");
            TokenType = response.Value<string>("token_type");
            RefreshToken = response.Value<string>("refresh_token");
            ExpiresIn = response.Value<string>("expires_in");
        }

        private OAuthTokenRes(Exception error)
        {
            Error = error;
        }

        public static OAuthTokenRes Success(JObject response)
        {
            return new OAuthTokenRes(response);
        }

        public static OAuthTokenRes Failed(Exception error)
        {
            return new OAuthTokenRes(error);
        }

        public JObject Response { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public string ExpiresIn { get; set; }
        public Exception Error { get; set; }
    }

}
