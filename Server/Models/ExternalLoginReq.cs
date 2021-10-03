using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class ExternalLoginReq
    {
        public string Code { get; set; }
        public string Provider { get; set; }
    }
}
