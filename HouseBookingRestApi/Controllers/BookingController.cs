using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HouseBookingRestApi.Controllers
{
    [ApiController]
    [Route("api/v1/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService bookingService;
        private readonly IHouseService houseService;

        public BookingController(IBookingService bookingService, IHouseService houseService)
        {
            this.bookingService = bookingService;
            this.houseService = houseService;
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(BookingReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<BookingReadOnlyDTO>> GetBookingById(int id)
        {

            var booking = await bookingService.GetBookingByIdAsync(id);
            if(booking == null)
            {
                throw new EntityNotFoundException("Booking not found");
            }
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            switch (currentUserRole)
            {
                case "Admin":
                    break;
                case "Renter":
                    var currentRenterId = int.Parse(User.FindFirst("RenterId")?.Value);
                    if (booking.RenterId != currentRenterId)
                    {
                        throw new EntityForbiddenException("You are not authorized to view this booking.");
                    }
                    break;
                case "Owner":
                    var currentOwnerId = int.Parse(User.FindFirst("OwnerId")?.Value);
                    var bookingHouse = await houseService.GetHouseByIdAsync(booking.HouseId);
                    var houseOwnerId = bookingHouse?.OwnerId;
                    if (houseOwnerId != currentOwnerId)
                    {
                        throw new EntityForbiddenException("You are not authorized to view this booking.");
                    }
                    break;
                default:
                    throw new EntityForbiddenException("You are not authorized to view this booking.");
            }
            return Ok(booking);
        }

        [HttpGet("by-renter/{renterId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<BookingReadOnlyDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<BookingReadOnlyDTO>>> GetBookingsByRenterId(int renterId)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if(currentUserRole != "Admin" && currentUserRole != "Renter")
            {
                throw new EntityForbiddenException("You are not authorized to view bookings by renter.");
            }
            if(currentUserRole == "Renter")
            {
                var currentRenterId = int.Parse(User.FindFirst("RenterId")?.Value);
                if (currentRenterId != renterId)
                {
                    throw new EntityForbiddenException("You are not authorized to view another renter's bookings.");
                }
            }
            

            var bookings = await bookingService.GetBookingsByRenterIdAsync(renterId);
            return Ok(bookings);
        }

        [HttpGet("by-house/{houseId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<BookingReadOnlyDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookingReadOnlyDTO>>> GetBookingsByHouseId(int houseId)
        {
            var house = await houseService.GetHouseByIdAsync(houseId);
            if (house == null)
            {
                throw new EntityNotFoundException("House not found");
            }

            var ownerId = house.OwnerId;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != "Admin" && currentUserRole != "Owner")
            {
                throw new EntityForbiddenException("You are not authorized to view bookings.");
            }
            var currentOwnerId = int.Parse(User.FindFirst("OwnerId")?.Value);
            if (currentUserRole == "Owner" && currentOwnerId != ownerId)
            {
                throw new EntityForbiddenException("You are not authorized to view these bookings.");
            }
            var bookings = await bookingService.GetBookingsByHouseIdAsync(houseId);
            return Ok(bookings);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CreateBooking([FromBody] BookingRegisterDTO dto)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != "Renter")
            {
                throw new EntityForbiddenException("You are not authorized to create a booking.");
            }
            var currentRenterId = int.Parse(User.FindFirst("RenterId")?.Value);
            if (currentRenterId != dto.RenterId)
            {
                throw new EntityForbiddenException("You are not authorized to create a booking for this renter.");
            }
            await bookingService.RegisterBookingAsync(dto);
            return Created();
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CancelBooking(int id)
        {
            var booking = await bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                throw new EntityNotFoundException("Booking not found");
            }
            
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            switch (currentUserRole)
            {
                case "Admin":
                    break;
                case "Renter":
                    var bookingRenterId = booking.RenterId;
                    var currentRenterId = int.Parse(User.FindFirst("RenterId")?.Value);
                    if (bookingRenterId != currentRenterId)
                    {
                        throw new EntityForbiddenException("You are not authorized to cancel this booking.");
                    }
                    if (booking.StartDate <= DateOnly.FromDateTime(DateTime.Now))
                    {
                        throw new EntityForbiddenException("Cannot cancel booking after Check in Date");
                    }
                    break;
                case "Owner":
                    var bookingHouse = await houseService.GetHouseByIdAsync(booking.HouseId);
                    var currentOwnerId = int.Parse(User.FindFirst("OwnerId")?.Value);
                    var houseOwnerId = bookingHouse?.OwnerId;
                    if (houseOwnerId != currentOwnerId)
                    {
                        throw new EntityForbiddenException("You are not authorized to cancel this booking.");
                    }
                    if (booking.StartDate <= DateOnly.FromDateTime(DateTime.Now))
                    {
                        throw new EntityForbiddenException("Cannot cancel booking after Check in Date");
                    }
                    break;
                default:
                    throw new EntityForbiddenException("You are not authorized to cancel this booking.");
            }

            
            var dto = new BookingCancelDTO(id);
            await bookingService.DeleteBooking(dto);
            return NoContent();
        }
    }
}
