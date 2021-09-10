using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Server.Helpers
{
    public class JwtCookieAuthenticationHandler : JwtBearerHandler
    {
        public JwtCookieAuthenticationHandler(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.ContainsKey(HeaderNames.Authorization)
            || !Request.Cookies.ContainsKey("token")
            )
            {
                return await base.HandleAuthenticateAsync();
            }

            Request.Headers[HeaderNames.Authorization] =
                $"Bearer {Request.Cookies["token"]}";
            var result = await base.HandleAuthenticateAsync();
            Request.Headers.Remove(HeaderNames.Authorization);
            return result;
        }
    }
}