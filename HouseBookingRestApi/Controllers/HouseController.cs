using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
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
        private readonly IUserService userService;
        public HouseController(IHouseService houseService, IUserService userService)
        {
            this.houseService = houseService;
            this.userService = userService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HouseReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HouseReadOnlyDTO>> GetHouseById(int id)
        {
            var house = await houseService.GetHouseByIdAsync(id);
            return Ok(house);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<HouseReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HouseReadOnlyDTO>>> GetAllHouses([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var houses = await houseService.GetPaginatedHousesAsync(page, size);
            return Ok(houses);
        }

        [HttpPost]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(typeof(HouseReadOnlyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HouseReadOnlyDTO>> CreateHouse([FromBody] HouseRegisterDTO dto)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != "Owner")
            {
                throw new EntityForbiddenException("Only users with the 'Owner' role can create houses.");
            }
            var createdHouse = await houseService.CreateHouseAsync(dto);
            return CreatedAtAction(nameof(GetHouseById), new { id = createdHouse.Id }, createdHouse);
        }

        [HttpGet("by-owner/{ownerId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<HouseReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HouseReadOnlyDTO>>> getAllOwnerHouses(int ownerId)
        {
            var houses = await houseService.GetHousesByOwnerId(ownerId);
            return Ok(houses);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrHouseOwner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteHouse(int id)
        {
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
