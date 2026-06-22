using HouseBookingRestApi.Controllers;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;
using Xunit;

namespace HouseBookingRestApi.Tests.Controllers
{
    public class BookingControllerTests
    {
        private readonly BookingController controller;
        private readonly IBookingService bookingService;
        private readonly IHouseService houseService;

        public BookingControllerTests()
        {
            bookingService = Substitute.For<IBookingService>();
            houseService = Substitute.For<IHouseService>();
            controller = new BookingController(bookingService, houseService);
        }

        private void SetupUserClaims(string role, int? ownerId = null, int? renterId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };
            if (ownerId.HasValue)
                claims.Add(new Claim("OwnerId", ownerId.Value.ToString()));
            if (renterId.HasValue)
                claims.Add(new Claim("RenterId", renterId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetBookingById_AsAdmin_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("Admin");
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            bookingService.GetBookingByIdAsync(1).Returns(booking);

            // Act
            var result = await controller.GetBookingById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooking = Assert.IsType<BookingReadOnlyDTO>(okResult.Value);
            Assert.Equal(1, returnedBooking.Id);
        }

        [Fact]
        public async Task GetBookingById_AsRenterOfBooking_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 1);
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            bookingService.GetBookingByIdAsync(1).Returns(booking);

            // Act
            var result = await controller.GetBookingById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooking = Assert.IsType<BookingReadOnlyDTO>(okResult.Value);
            Assert.Equal(1, returnedBooking.Id);
        }

        [Fact]
        public async Task GetBookingById_AsRenterOfDifferentBooking_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 2);
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            bookingService.GetBookingByIdAsync(1).Returns(booking);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.GetBookingById(1)
            );
        }

        [Fact]
        public async Task GetBookingById_AsOwnerOfHouse_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            bookingService.GetBookingByIdAsync(1).Returns(booking);
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act
            var result = await controller.GetBookingById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooking = Assert.IsType<BookingReadOnlyDTO>(okResult.Value);
            Assert.Equal(1, returnedBooking.Id);
        }

        [Fact]
        public async Task GetBookingById_AsOwnerOfDifferentHouse_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 2);
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            bookingService.GetBookingByIdAsync(1).Returns(booking);
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.GetBookingById(1)
            );
        }

        [Fact]
        public async Task GetBookingById_WithNonExistentBooking_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("Admin");
            bookingService.GetBookingByIdAsync(999).Returns((BookingReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.GetBookingById(999)
            );
        }

        [Fact]
        public async Task GetBookingsByRenterId_AsRenterForOwnBookings_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 1);
            var bookings = new List<BookingReadOnlyDTO>
            {
                new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House")
            };
            bookingService.GetBookingsByRenterIdAsync(1).Returns(bookings);

            // Act
            var result = await controller.GetBookingsByRenterId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBookings = Assert.IsType<List<BookingReadOnlyDTO>>(okResult.Value);
            Assert.Single(returnedBookings);
        }

        [Fact]
        public async Task GetBookingsByRenterId_AsRenterForOtherRenter_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 2);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.GetBookingsByRenterId(1)
            );
        }

        [Fact]
        public async Task GetBookingsByRenterId_AsAdmin_ReturnsOk()
        {
            // Arrange
            // Note: Admin needs RenterId claim due to controller parsing it before role check
            SetupUserClaims("Admin", renterId: 1);
            var bookings = new List<BookingReadOnlyDTO>
            {
                new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House")
            };
            bookingService.GetBookingsByRenterIdAsync(1).Returns(bookings);

            // Act
            var result = await controller.GetBookingsByRenterId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBookings = Assert.IsType<List<BookingReadOnlyDTO>>(okResult.Value);
            Assert.Single(returnedBookings);
        }

        [Fact]
        public async Task GetBookingsByHouseId_AsOwnerOfHouse_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            var bookings = new List<BookingReadOnlyDTO>
            {
                new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House")
            };
            houseService.GetHouseByIdAsync(1).Returns(house);
            bookingService.GetBookingsByHouseIdAsync(1).Returns(bookings);

            // Act
            var result = await controller.GetBookingsByHouseId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBookings = Assert.IsType<List<BookingReadOnlyDTO>>(okResult.Value);
            Assert.Single(returnedBookings);
        }

        [Fact]
        public async Task GetBookingsByHouseId_AsOwnerOfDifferentHouse_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 2);
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.GetBookingsByHouseId(1)
            );
        }

        [Fact]
        public async Task GetBookingsByHouseId_WithNonExistentHouse_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("Admin");
            houseService.GetHouseByIdAsync(999).Returns((HouseReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.GetBookingsByHouseId(999)
            );
        }

        [Fact]
        public async Task CreateBooking_AsRenter_ReturnsCreated()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 1);
            var dto = new BookingRegisterDTO(1, 1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5));
            bookingService.RegisterBookingAsync(dto).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CreateBooking(dto);

            // Assert
            Assert.IsType<CreatedResult>(result);
        }

        [Fact]
        public async Task CreateBooking_AsRenterForDifferentRenter_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 2);
            var dto = new BookingRegisterDTO(1, 1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5));

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.CreateBooking(dto)
            );
        }

        [Fact]
        public async Task CreateBooking_AsOwner_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var dto = new BookingRegisterDTO(1, 1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5));

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.CreateBooking(dto)
            );
        }

        [Fact]
        public async Task CancelBooking_AsAdmin_ReturnsNoContent()
        {
            // Arrange
            SetupUserClaims("Admin");
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            bookingService.GetBookingByIdAsync(1).Returns(booking);
            bookingService.DeleteBooking(Arg.Any<BookingCancelDTO>()).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CancelBooking(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CancelBooking_AsRenterBeforeCheckIn_ReturnsNoContent()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 1);
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            bookingService.GetBookingByIdAsync(1).Returns(booking);
            bookingService.DeleteBooking(Arg.Any<BookingCancelDTO>()).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CancelBooking(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CancelBooking_AsRenterAfterCheckIn_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter", renterId: 1);
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            bookingService.GetBookingByIdAsync(1).Returns(booking);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.CancelBooking(1)
            );
        }

        [Fact]
        public async Task CancelBooking_AsOwnerBeforeCheckIn_ReturnsNoContent()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var booking = new BookingReadOnlyDTO(1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), 1, 1, "Test House");
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            bookingService.GetBookingByIdAsync(1).Returns(booking);
            houseService.GetHouseByIdAsync(1).Returns(house);
            bookingService.DeleteBooking(Arg.Any<BookingCancelDTO>()).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CancelBooking(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CancelBooking_WithNonExistentBooking_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("Admin");
            bookingService.GetBookingByIdAsync(999).Returns((BookingReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.CancelBooking(999)
            );
        }
    }
}
