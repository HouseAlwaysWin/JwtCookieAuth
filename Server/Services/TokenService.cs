using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Server.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public class TokenService : ITokenService
    {
        private readonly IAntiforgery _antiforgery;
        public TokenService(IAntiforgery antiforgery)
        {
            this._antiforgery = antiforgery;
        }

        public string CreateJwtToken(string secretKey, string issuer, List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(1),
                SigningCredentials = creds,
                Issuer = issuer
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenstring = tokenHandler.WriteToken(token);
            return tokenstring;
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            return refreshToken;
        }

        /// <summary>
        /// 產生Csrf Token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public string GenerateCsrfToken(HttpContext httpContext)
        {
            var res = this._antiforgery.GetAndStoreTokens(httpContext);
            if (!httpContext.Request.Cookies.ContainsKey("XSRF-TOKEN"))
            {
                httpContext.Response.Cookies.Append("XSRF-TOKEN", res.CookieToken, new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    Secure = true
                });
            }

            return res.RequestToken;
        }


    }
}
