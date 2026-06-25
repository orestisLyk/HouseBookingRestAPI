using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBookingRestApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IConfiguration configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            this.userService = userService;
            this.configuration = configuration;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if the credentials are valid.
        /// </summary>
        /// <param name="dto">The user login data transfer object containing username and password.</param>
        /// <returns>A JWT token if authentication is successful.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(JwtTokenDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<JwtTokenDTO>> Login([FromBody] UserLoginDTO dto)
        {
            var jwtDTO = await userService.Login(dto);

            return Ok(jwtDTO);
        }


        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="dto">The user registration data transfer object containing user details.</param>
        /// <returns>The registered user data transfer object.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            await userService.RegisterUserAsync(dto);
            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Creates an admin user in the system according to the configured admin credentials. This endpoint is intended for initial setup.
        /// </summary>
        /// <returns></returns>
        [HttpPost("admin")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateAdmin()
        {
            await userService.CreateAdminAsync();
            return Created("", "Admin user created successfully.");
        }


    }
}
