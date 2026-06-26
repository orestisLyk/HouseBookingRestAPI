using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HouseBookingRestApi.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Gets the profile of a user by their username.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>The user's profile information.</returns>
        [HttpGet("{username}")]
        [Authorize]
        [ProducesResponseType(typeof(UserReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserReadOnlyDTO>> GetUserByUsername(string username)
        {
            EnsureCanViewUser(username);
            var returnDto = await userService.GetUserByUsernameAsync(username);
            return Ok(returnDto);
        }

        /// <summary>
        /// Gets all users in the system.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="size">The number of users per page.</param>
        /// <returns>A paginated list of users.</returns>
        /// <exception cref="EntityForbiddenException"></exception>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PaginatedResult<UserReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<UserReadOnlyDTO>>> GetUsers([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "ManageUsers"))
            {
                throw new EntityForbiddenException("You are not authorized to view the list of users.");
            }
            var paginatedUsers = await userService.GetPaginatedUsersAsync(page, size);
            return Ok(paginatedUsers);
        }


        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        /// <exception cref="EntityForbiddenException"></exception>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var dto = new UserDeleteDTO(id);
            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "ManageUsers"))
            {
                throw new EntityForbiddenException("You are not authorized to delete users.");
            }

            await userService.DeleteUserAsync(dto);
            return NoContent();
        }

        private void EnsureCanViewUser(string username)
        {
            var currentUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            var isOwnProfile = string.Equals(currentUsername, username, StringComparison.OrdinalIgnoreCase);
            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!isOwnProfile && !currentUserCapabilities.Any(c => c == "ManageUsers"))
            {
                throw new EntityForbiddenException("You are not authorized to view this user's profile.");
            }
        }
    }
}
