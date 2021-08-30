using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class OAuthLoginParam
    {
        public string Provider { get; set; }
        public string Code { get; set; }
    }
}
