using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HouseBookingRestApi.Controllers
{
    [ApiController]
    [Route("api/v1/houses")]
    public class HouseController : ControllerBase
    {
        private readonly IHouseService houseService;
        public HouseController(IHouseService houseService)
        {
            this.houseService = houseService;
        }

        /// <summary>
        /// Get a House by its ID.
        /// </summary>
        /// <param name="id">The ID of the house.</param>
        /// <returns>The house details.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HouseReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HouseReadOnlyDTO>> GetHouseById(int id)
        {
            var house = await houseService.GetHouseByIdAsync(id);
            return Ok(house);
        }

        /// <summary>
        /// Get all houses with pagination.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="size">The number of houses per page.</param>
        /// <returns>A paginated list of houses.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<HouseReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HouseReadOnlyDTO>>> GetAllHouses([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var houses = await houseService.GetPaginatedHousesAsync(page, size);
            return Ok(houses);
        }

        /// <summary>
        /// Creates a new house.
        /// </summary>
        /// <param name="dto">The house registration details.</param>
        /// <returns>The created house details.</returns>
        /// <exception cref="EntityForbiddenException"></exception>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(HouseReadOnlyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HouseReadOnlyDTO>> CreateHouse([FromBody] HouseRegisterDTO dto)
        {
            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "CreateHouse"))
            {
                throw new EntityForbiddenException("You are not authorized to create houses.");
            }

            var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
            if (ownerIdClaim == null)
            {
                throw new EntityForbiddenException("Owner ID not found in token.");
            }

            var houseDto = dto with { OwnerId = int.Parse(ownerIdClaim) };

            var createdHouse = await houseService.CreateHouseAsync(houseDto);
            return CreatedAtAction(nameof(GetHouseById), new { id = createdHouse.Id }, createdHouse);
        }

        /// <summary>
        /// Gets all Houses owned by the authenticated owner.
        /// </summary>
        /// <param name="ownerId">The ID of the owner.</param>
        /// <returns>A list of houses owned by the specified owner.</returns>
        [HttpGet("by-owner/{ownerId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<HouseReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HouseReadOnlyDTO>>> getAllOwnerHouses(int ownerId)
        {
            var houses = await houseService.GetHousesByOwnerId(ownerId);
            return Ok(houses);
        }

        /// <summary>
        /// Deletes a house by its ID.
        /// </summary>
        /// <param name="id">The ID of the house to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        /// <exception cref="EntityForbiddenException"></exception>
        /// <exception cref="EntityNotFoundException"></exception>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteHouse(int id)
        {
            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "DeleteHouse"))
            {
                throw new EntityForbiddenException("You are not authorized to delete houses.");
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            switch (currentUserRole)
            {
                case "Admin":
                    break; // Admin can delete any house
                case "Owner":
                    var house = await houseService.GetHouseByIdAsync(id);
                    if (house == null)
                    {
                        throw new EntityNotFoundException($"House with ID {id} not found.");
                    }

                    // Get OwnerId from claims
                    var ownerIdClaim = User.FindFirst("OwnerId")?.Value;
                    if (ownerIdClaim == null)
                    {
                        throw new EntityForbiddenException("Owner ID not found in token.");
                    }
                    int currentOwnerId = int.Parse(ownerIdClaim);

                    if (house.OwnerId != currentOwnerId)
                    {
                        throw new EntityForbiddenException("You are not authorized to delete this house.");
                    }
                    break;
                default:
                    throw new EntityForbiddenException("You are not authorized to delete houses.");
            }
            await houseService.DeleteHouseAsync(id);
            return NoContent();
        }
    }
}
