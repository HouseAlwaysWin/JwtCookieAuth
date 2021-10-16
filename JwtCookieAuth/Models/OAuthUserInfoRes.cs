using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Models
{
    public class OAuthUserInfoRes
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }
        public string Provider { get; set; }

    }
}
