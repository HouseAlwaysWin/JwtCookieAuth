using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Middleware
{
    public class ValidateAntiForgeryTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiForgery;

        public ValidateAntiForgeryTokenMiddleware(RequestDelegate next, IAntiforgery antiForgery)
        {
            _next = next;
            _antiForgery = antiForgery;
        }

        public async Task Invoke(HttpContext context)
        {
            if (HttpMethods.IsGet(context.Request.Method))
            {
                var result = _antiForgery.GetAndStoreTokens(context);
            }

            if (HttpMethods.IsPost(context.Request.Method))
            {
                try
                {
                    await _antiForgery.ValidateRequestAsync(context);
                }catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            await _next(context);
        }
    }

}
