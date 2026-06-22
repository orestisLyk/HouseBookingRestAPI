using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HouseBookingRestApi.Controllers
{
    [ApiController]
    [Route("api/v1/house-images")]
    public class HouseImageController : ControllerBase
    {
        private readonly IHouseImageService imageService;
        private readonly IHouseService houseService;

        public HouseImageController(IHouseImageService imageService, IHouseService houseService)
        {
            this.imageService = imageService;
            this.houseService = houseService;
        }

        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreateImage([FromForm] HouseImageCreateDTO dto)
        {
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if(currentRole != "Owner")
            {
                throw new EntityForbiddenException("Only owners can create house images");
            }
            var house = await houseService.GetHouseByIdAsync(dto.HouseId);
            if(house == null)
            {
                throw new EntityNotFoundException("House not found");
            }
            var ownerId = house.OwnerId;
            var currentOwnerId = int.Parse(User.FindFirst("OwnerId")?.Value);
            if(ownerId != currentOwnerId)
            {
                throw new EntityForbiddenException("Cannot create an image for another owners house");
            }
            await imageService.CreateImageAsync(dto);
            return Created();
        }

        [HttpGet("by-house/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<HouseImageReadOnlyDTO>>> GetImagesByHouseId(int id)
        {
            var images = await imageService.GetImagesByHouseIdAsync(id);
            return Ok(images);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HouseImageReadOnlyDTO>> GetImageById(int id)
        {
            var image = await imageService.GetImageByIdAsync(id);
            if (image == null)
            {
                throw new EntityNotFoundException("Image not found");
            }
            return Ok(image);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteImage(int id)
        {
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentRole != "Owner")
            {
                throw new EntityForbiddenException("Only owners can delete house images");
            }
            var image = await imageService.GetImageByIdAsync(id);
            if (image == null)
            {
                throw new EntityNotFoundException("Image not found");
            }
            var house = await houseService.GetHouseByIdAsync(image.HouseId);
            if (house == null)
            {
                throw new EntityNotFoundException("House not found");
            }
            var ownerId = house.OwnerId;
            var currentOwnerId = int.Parse(User.FindFirst("OwnerId")?.Value);
            if(ownerId != currentOwnerId)
            {
                throw new EntityForbiddenException("Cannot delete an image for another owners house");
            }
            await imageService.DeleteImageAsync(id);
            return NoContent();
        }
    }
}
