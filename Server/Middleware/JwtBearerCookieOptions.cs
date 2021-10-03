using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Middleware
{
    public class JwtBearerCookieOptions : JwtBearerOptions
    {
        public string JwtTokenName { get; set; } = "jwt_token";
        public string JwtRefreshTokenName { get; set; } = "refresh_token";
    }
}
