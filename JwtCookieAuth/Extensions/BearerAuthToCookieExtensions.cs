using JwtCookieAuth.Middlewares;
using JwtCookieAuth.Models;
using JwtCookieAuth.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Extensions
{
    public static class BearerAuthToCookieExtensions
    {
        public static AuthenticationBuilder UseJwtAuthToCookie(this AuthenticationBuilder builder, Action<JwtCookieBearerOptions> jwtOptions, Action<AntiforgeryOptions> csrfOptions = null)
        {
            builder.AddSchemeOptionsHandler<JwtCookieBearerOptions, JwtCookieAuthHandler>(JwtBearerDefaults.AuthenticationScheme, jwtOptions);
            builder.Services.AddScoped<IJwtCookieAuthService, JwtCookieAuthService>();
            builder.Services.AddSingleton<ICachedService, CachedService>();
            if (csrfOptions == null)
            {
                builder.Services.AddAntiforgery(csrfOptions =>
                {
                    csrfOptions.Cookie.Name = "XSRF-TOKEN";
                    csrfOptions.HeaderName = "X-XSRF-TOKEN";
                    csrfOptions.Cookie.HttpOnly = false;
                    csrfOptions.SuppressXFrameOptionsHeader = false;
                });
            }
            else
            {
                builder.Services.AddAntiforgery(csrfOptions);
            }

            return builder;
        }

        public static AuthenticationBuilder UseOAuthProvider(this AuthenticationBuilder builder, OAuthProviderEnum provider, Action<OAuthConfigOptions> options)
        {
            builder.AddOAuthConfig(provider.ToString(), options);
            return builder;
        }

        public static AuthenticationBuilder UseOAuthProvider(this AuthenticationBuilder builder, string provider, Action<OAuthConfigOptions> options)
        {
            builder.AddOAuthConfig(provider, options);
            return builder;
        }

        public static AuthenticationBuilder UseOAuthProvider(this AuthenticationBuilder builder, OAuthProviderEnum provider, IConfiguration config)
        {
            builder.Services.Configure<OAuthConfigOptions>(provider.ToString(), config.GetSection(provider.ToString()));
            return builder;
        }

        public static AuthenticationBuilder UseOAuthProvider<T>(this AuthenticationBuilder builder, OAuthProviderEnum provider, IConfiguration config)
            where T : OAuthConfigOptions
        {
            builder.Services.Configure<T>(provider.ToString(), config.GetSection(provider.ToString()));
            return builder;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="builder"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="options"></param>
        public static AuthenticationBuilder AddSchemeOptionsHandler<TOptions, THandler>(this AuthenticationBuilder builder, string authenticationScheme, Action<TOptions> options)
           where TOptions : JwtCookieBearerOptions, new()
           where THandler : class, IAuthenticationHandler
        {

            builder.Services.Configure<AuthenticationOptions>(o =>
            {
                o.AddScheme(authenticationScheme, scheme =>
                {
                    scheme.HandlerType = typeof(THandler);
                    scheme.DisplayName = null;
                });
            });

            if (options != null)
            {
                builder.Services.Configure(authenticationScheme, options);
                builder.Services.Configure(options);
            }
            builder.Services.AddTransient<THandler>();
            return builder;
        }

        /// <summary>
        /// 加入OAuth 設定
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="providerName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddOAuthConfig<TOptions>(this AuthenticationBuilder builder, string providerName, Action<TOptions> options)
           where TOptions : OAuthConfigOptions, new()
        {
            if (options != null)
            {
                builder.Services.Configure(providerName, options);
            }
            return builder;
        }

    }
}
