using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Models
{
    public class GoogleTokenRes : OAuthTokenRes
    {
        public GoogleTokenRes(OAuthTokenRes res) : base(res.AccessToken, res.TokenType, res.RefreshToken, res.ExpiresIn)
        {
            IdToken = res.Response.Value<string>("id_token");
        }

        public string IdToken { get; set; }
    }

}
