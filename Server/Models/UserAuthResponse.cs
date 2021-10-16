using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class UserAuthResponse
    {
        public bool IsAuth { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
