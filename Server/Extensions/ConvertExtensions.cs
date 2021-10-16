using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Server.Extensions
{
    public static class ConvertExtensions
    {
        public static UserInfo ConvertToUserInfo(this IEnumerable<Claim> claims)
        {
            var name = claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            var email = claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var pictureUrl = claims.FirstOrDefault(c => c.Type == "PictureUrl")?.Value;
            var id = claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            var provider = claims.FirstOrDefault(c => c.Type == "Provider")?.Value;

            return new UserInfo
            {
                Id = id,
                Name = name,
                Email = email,
                PictureUrl = pictureUrl,
                Provider = provider
            };
        }
    }
}
