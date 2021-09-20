using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class AuthResponse
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }
        public string Provider { get; set; }
    }
}
