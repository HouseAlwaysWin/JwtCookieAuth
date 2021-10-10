using System;
using System.Text;
using JwtCookieAuth.Extensions;
using JwtCookieAuth.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server.Extensions;
using Server.Services;

namespace Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public IConfiguration _config { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            services.AddControllers();

            var Issuer = _config["Token:Issuer"];
            var tokenKey = _config["Token:Key"];


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .UseBearerAuthToCookie(
                    jwtOptions =>
                    {
                        jwtOptions.JwtKey = "0s9e8g7h7e739348dgjioiewrjf";
                        jwtOptions.JwtIssuer = Issuer;

                        jwtOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                            ValidIssuer = Issuer,
                            ValidateIssuer = true,
                            ValidateAudience = false,
                        };
                    },
                    csrfOptions =>
                    {
                        csrfOptions.Cookie.Name = "XSRF-TOKEN";
                        csrfOptions.HeaderName = "X-XSRF-TOKEN";
                        csrfOptions.Cookie.HttpOnly = false;
                        csrfOptions.SuppressXFrameOptionsHeader = false;
                    })
               .UseOAuthProvider(OAuthProviderEnum.Google, options =>
               {
                   options.ClientId = "563822801624-brvnu1sftmi78lfntvkk9s38jc4ubke7.apps.googleusercontent.com";
                   options.ClientSecret = "Q0H3EoSU8eXC2aaLPTqrmMnP";
                   options.TokenEndpoint = "https://oauth2.googleapis.com/token";
                   options.RedirectUrl = "http://localhost:4200/callback/google";
               })
                .UseOAuthProvider(OAuthProviderEnum.Line, options =>
                {
                    options.ClientId = "1656348599";
                    options.ClientSecret = "7e4e1bac01f88708e60544219c6c6214";
                    options.TokenEndpoint = "https://api.line.me/oauth2/v2.1/token";
                    options.RedirectUrl = "http://localhost:4200/callback/line";
                    options.UserInformationEndpoint = "https://api.line.me/v2/profile";
                });



            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpClient<IAuthService, AuthService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddAutoMapper(typeof(AutoMapperProfile));



            services.AddCors();

            services.AddMvc();
            //services.AddAntiforgery(options =>
            //{
            //    options.Cookie.Name = "XSRF-TOKEN";
            //    options.HeaderName = "X-XSRF-TOKEN";
            //    options.Cookie.HttpOnly = false;
            //    options.SuppressXFrameOptionsHeader = false;
            //});


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Server", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAntiforgery antiforgery)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Server v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(opt =>
                opt.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins(
                   "http://localhost:4200"));


            app.UseAuthentication();
            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
