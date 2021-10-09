using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Models
{
    public class JwtCookieBearerOptions : JwtBearerOptions

    {
        public string JwtName { get; set; } = "jwt_token";
        public string JwtKey { get; set; }
        public string JwtIssuer { get; set; }
        public int JwtExpires { get; set; } = 5;
        public string RefreshTokenName { get; set; } = "refresh_token";
        public string RefreshTokensAllCachedName { get; set; } = "all_refresh_tokens";
        public int RefreshTokenExpires { get; set; } = 1440;
    }
}
