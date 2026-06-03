using HouseRentingSystemApi.Models.Authorization;
using HouseRentingSystemApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HouseRentingSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(typeof(AuthResult))]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return Unauthorized(new AuthResult { Code = 400, Massage = "Невалидни данни.", Token = string.Empty });
            }

            var result = await authService.LoginAsync(model);

            if (result.Code != 200)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult { Code = 400, Massage = "Невалидни данни.", Token = string.Empty });
            }

            var result = await authService.RegisterAsync(model);

            if (result.Code != 200)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
