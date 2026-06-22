using AutoMapper;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Service;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HouseBookingRestApi.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly BookingService service;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<BookingService> logger;

        public BookingServiceTests()
        {
            unitOfWork = Substitute.For<IUnitOfWork>();
            mapper = Substitute.For<IMapper>();
            logger = Substitute.For<ILogger<BookingService>>();
            service = new BookingService(unitOfWork, mapper, logger);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithValidId_ReturnsBookingDTO()
        {
            // Arrange
            var booking = new Booking { Id = 1, HouseId = 1, RenterId = 1, IsDeleted = false };
            var bookingDTO = new BookingReadOnlyDTO(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), 1, 1, "Test House");

            unitOfWork.BookingRepository.GetBookingByIdAsync(1).Returns(booking);
            mapper.Map<BookingReadOnlyDTO>(booking).Returns(bookingDTO);

            // Act
            var result = await service.GetBookingByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            await unitOfWork.BookingRepository.Received(1).GetBookingByIdAsync(1);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNullBooking_ThrowsEntityNotFoundException()
        {
            // Arrange
            unitOfWork.BookingRepository.GetBookingByIdAsync(999).Returns((Booking?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.GetBookingByIdAsync(999)
            );
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithDeletedBooking_ThrowsEntityNotFoundException()
        {
            // Arrange
            var booking = new Booking { Id = 1, HouseId = 1, RenterId = 1, IsDeleted = true };
            unitOfWork.BookingRepository.GetBookingByIdAsync(1).Returns(booking);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.GetBookingByIdAsync(1)
            );
        }

        [Fact]
        public async Task GetBookingsByHouseIdAsync_ReturnsBookingList()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, HouseId = 1, RenterId = 1 },
                new Booking { Id = 2, HouseId = 1, RenterId = 2 }
            };
            var bookingDTOs = new List<BookingReadOnlyDTO>
            {
                new BookingReadOnlyDTO(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), 1, 1, "House 1"),
                new BookingReadOnlyDTO(2, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), 1, 2, "House 1")
            };

            unitOfWork.BookingRepository.GetBookingsByHouseIdAsync(1).Returns(bookings);
            mapper.Map<List<BookingReadOnlyDTO>>(bookings).Returns(bookingDTOs);

            // Act
            var result = await service.GetBookingsByHouseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetBookingsByRenterIdAsync_ReturnsBookingList()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, HouseId = 1, RenterId = 1 },
                new Booking { Id = 2, HouseId = 2, RenterId = 1 }
            };
            var bookingDTOs = new List<BookingReadOnlyDTO>
            {
                new BookingReadOnlyDTO(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), 1, 1, "House 1"),
                new BookingReadOnlyDTO(2, DateTime.UtcNow, DateTime.UtcNow.AddDays(2), 2, 1, "House 2")
            };

            unitOfWork.BookingRepository.GetBookingsByRenterIdAsync(1).Returns(bookings);
            mapper.Map<List<BookingReadOnlyDTO>>(bookings).Returns(bookingDTOs);

            // Act
            var result = await service.GetBookingsByRenterIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task RegisterBookingAsync_WithValidData_CreatesBooking()
        {
            // Arrange
            var dto = new BookingRegisterDTO(1, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));
            var house = new House { Id = 1, Name = "Test House" };
            var renter = new Renter { Id = 1 };
            var booking = new Booking { Id = 1, HouseId = 1, RenterId = 1 };

            unitOfWork.HouseRepository.GetHouseByIdAsync(1).Returns(house);
            unitOfWork.RenterRepository.GetRenterByIdAsync(1).Returns(renter);
            unitOfWork.BookingRepository.GetBookingsByHouseIdAsync(1).Returns(new List<Booking>());
            mapper.Map<Booking>(dto).Returns(booking);

            // Act
            await service.RegisterBookingAsync(dto);

            // Assert
            await unitOfWork.BookingRepository.Received(1).AddAsync(Arg.Any<Booking>());
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task RegisterBookingAsync_WithPastStartDate_ThrowsInvalidBookingDatesException()
        {
            // Arrange
            var dto = new BookingRegisterDTO(1, 1, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidBookingDatesException>(
                async () => await service.RegisterBookingAsync(dto)
            );
        }

        [Fact]
        public async Task RegisterBookingAsync_WithEndDateBeforeStartDate_ThrowsInvalidBookingDatesException()
        {
            // Arrange
            var dto = new BookingRegisterDTO(1, 1, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(1));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidBookingDatesException>(
                async () => await service.RegisterBookingAsync(dto)
            );
        }

        [Fact]
        public async Task RegisterBookingAsync_WithNonExistentHouse_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new BookingRegisterDTO(999, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));
            unitOfWork.HouseRepository.GetHouseByIdAsync(999).Returns((House?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.RegisterBookingAsync(dto)
            );
        }

        [Fact]
        public async Task RegisterBookingAsync_WithNonExistentRenter_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new BookingRegisterDTO(1, 999, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));
            var house = new House { Id = 1, Name = "Test House" };

            unitOfWork.HouseRepository.GetHouseByIdAsync(1).Returns(house);
            unitOfWork.RenterRepository.GetRenterByIdAsync(999).Returns((Renter?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.RegisterBookingAsync(dto)
            );
        }

        [Fact]
        public async Task RegisterBookingAsync_WithOverlappingDates_ThrowsBookingsOverlapException()
        {
            // Arrange
            var dto = new BookingRegisterDTO(1, 1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));
            var house = new House { Id = 1, Name = "Test House" };
            var renter = new Renter { Id = 1 };
            var existingBooking = new Booking 
            { 
                Id = 1, 
                HouseId = 1, 
                StartDate = DateTime.UtcNow.AddDays(2), 
                EndDate = DateTime.UtcNow.AddDays(4) 
            };

            unitOfWork.HouseRepository.GetHouseByIdAsync(1).Returns(house);
            unitOfWork.RenterRepository.GetRenterByIdAsync(1).Returns(renter);
            unitOfWork.BookingRepository.GetBookingsByHouseIdAsync(1).Returns(new List<Booking> { existingBooking });

            // Act & Assert
            await Assert.ThrowsAsync<BookingsOverlapException>(
                async () => await service.RegisterBookingAsync(dto)
            );
        }

        [Fact]
        public async Task DeleteBooking_WithValidId_MarksAsDeleted()
        {
            // Arrange
            var dto = new BookingCancelDTO(1);
            var booking = new Booking { Id = 1, HouseId = 1, RenterId = 1, IsDeleted = false };

            unitOfWork.BookingRepository.GetBookingByIdAsync(1).Returns(booking);

            // Act
            await service.DeleteBooking(dto);

            // Assert
            Assert.True(booking.IsDeleted);
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task DeleteBooking_WithInvalidId_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new BookingCancelDTO(999);
            unitOfWork.BookingRepository.GetBookingByIdAsync(999).Returns((Booking?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.DeleteBooking(dto)
            );
        }
    }
}
