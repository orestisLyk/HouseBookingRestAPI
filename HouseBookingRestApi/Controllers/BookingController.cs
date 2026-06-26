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

        /// <summary>
        /// Gets a Booking by its ID.
        /// </summary>
        /// <param name="id">The ID of the booking.</param>
        /// <returns>The booking details.</returns>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="EntityForbiddenException"></exception>
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

            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "ViewBooking"))
            {
                throw new EntityForbiddenException("You are not authorized to access this booking");
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

        /// <summary>
        /// Gets All Bookings made by a Renter.
        /// </summary>
        /// <param name="renterId">The ID of the renter.</param>
        /// <returns>A list of bookings made by the specified renter.</returns>
        /// <exception cref="EntityForbiddenException"></exception>
        [HttpGet("by-renter/{renterId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<BookingReadOnlyDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<BookingReadOnlyDTO>>> GetBookingsByRenterId(int renterId)
        {
            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "ViewBooking"))
            {
                throw new EntityForbiddenException("You are not authorized to access this booking");
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
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

        /// <summary>
        /// Gets All Bookings made for a House.
        /// </summary>
        /// <param name="houseId">The ID of the house.</param>
        /// <returns>A list of bookings made for the specified house.</returns>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="EntityForbiddenException"></exception>
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

            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "ViewBooking"))
            {
                throw new EntityForbiddenException("You are not authorized to view these Bookings");
            }

            var ownerId = house.OwnerId;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentOwnerId = int.Parse(User.FindFirst("OwnerId")?.Value);
            if (currentUserRole == "Owner" && currentOwnerId != ownerId)
            {
                throw new EntityForbiddenException("You are not authorized to view these bookings.");
            }
            var bookings = await bookingService.GetBookingsByHouseIdAsync(houseId);
            return Ok(bookings);
        }

        /// <summary>
        /// Creates a new Booking.
        /// </summary>
        /// <param name="dto">The booking details.</param>
        /// <returns></returns>
        /// <exception cref="EntityForbiddenException"></exception>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CreateBooking([FromBody] BookingRegisterDTO dto)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserCapabilities = User.FindAll("Capabilities").Select(c => c.Value).ToList();
            if (!currentUserCapabilities.Any(c => c == "CreateBooking"))
            {
                throw new EntityForbiddenException("You are not authorized to create a booking");
            }

            var currentRenterId = int.Parse(User.FindFirst("RenterId")?.Value);
            if (currentRenterId != dto.RenterId)
            {
                throw new EntityForbiddenException("You are not authorized to create a booking for this renter.");
            }
            await bookingService.RegisterBookingAsync(dto);
            return Created();
        }

        /// <summary>
        /// Deletes a Booking by ID.
        /// </summary>
        /// <param name="id">The ID of the booking to delete.</param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="EntityForbiddenException"></exception>
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
