using System.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Server.Models;
using Server.OauthPrividers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Server.Services;
using Microsoft.Extensions.Logging;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;


        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            this._authService = authService;
            this._logger = logger;
        }


        [HttpGet("getCsrfToken")]
        public ActionResult GetCsrfToken()
        {
            var token = this._authService.GetAntiCsrfToken(HttpContext);
            return Ok(new { token = token });
        }


        [Authorize]
        [HttpGet("logout")]
        public ActionResult Logout()
        {
            _authService.Logout(HttpContext);
            return Ok();
        }


        [HttpPost("ExternalLogin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLogin(string code, string provider)
        {
            try
            {
                var userInfo = await _authService.ExternalLogin(code, provider, HttpContext);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return BadRequest();

        }



        [HttpGet("isAuth")]
        public ActionResult IsAuth()
        {
            bool isAuth = User.Identity.IsAuthenticated;
            return Ok(isAuth);
        }

    }
}