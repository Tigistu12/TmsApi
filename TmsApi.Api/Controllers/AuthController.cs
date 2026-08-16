using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Auth.DTOs;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(
        [FromBody] LoginRequest request,
        [FromServices] IWebHostEnvironment env)
    {
        // Demo credentials for Module 10 transport testing
        if (request.Username == "admin" && request.Password == "Password123!")
        {
            var dummyJwt = "header.payload.signature-demo-token";

            // Append HttpOnly authentication cookie — JavaScript CANNOT read this token
            Response.Cookies.Append("tms_auth", dummyJwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = !env.IsDevelopment(), // HTTP permitted locally in development
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            return Ok(new UserProfileDto("System Admin", "Admin"));
        }

        return Unauthorized(new { detail = "Invalid username or password." });
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // Inspect cookie attached automatically by the browser
        if (Request.Cookies.TryGetValue("tms_auth", out _))
        {
            return Ok(new UserProfileDto("System Admin", "Admin"));
        }

        return Unauthorized(new { detail = "Session expired or missing authentication cookie." });
    }
}