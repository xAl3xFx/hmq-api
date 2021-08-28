using System;
using System.Collections.Generic;

using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using HandleMyQueue.Models;
using HandleMyQueue.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HandleMyQueue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [Authorize]
        [HttpGet("checkIfAuthenticated")]
        public ActionResult CheckIfAuthenticated()
        {
            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public ActionResult Logout()
        {
           _authenticationService.Logout(Request, Response);
           return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            var result = await _authenticationService.Login(loginDto, Request, Response);
            return result ? Ok() : new BadRequestObjectResult("Invalid username or password.");
        }
    }
}