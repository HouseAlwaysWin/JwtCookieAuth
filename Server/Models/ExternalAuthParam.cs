using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class ExternalAuthParam
    {
        public string Provider { get; set; }
        public string Code { get; set; }
        public string RedirectUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TokenApiURI { get; set; }
    }
}
