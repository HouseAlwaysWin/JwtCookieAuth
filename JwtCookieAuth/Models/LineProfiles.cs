using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Models
{
    public class LineProfileRes
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string PictureUrl { get; set; }
        public string StatueMessage { get; set; }
    }

}
