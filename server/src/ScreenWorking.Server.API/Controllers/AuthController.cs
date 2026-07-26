using Microsoft.AspNetCore.Mvc;
using ScreenWorking.Server.API.Services;

namespace ScreenWorking.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService authService;

        public AuthController(AuthenticationService authService)
        {
            this.authService = authService;
        }

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username))
            {
                return BadRequest("Username is required.");
            }

            string token = authService.GenerateJwtToken(Guid.NewGuid().ToString("N"), request.Username, "Editor");
            return Ok(new { Token = token, Username = request.Username });
        }
    }
}
