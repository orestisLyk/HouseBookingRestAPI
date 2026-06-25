using AutoMapper;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Exceptions;

namespace HouseBookingRestApi.Service
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<BookingService> logger;
        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BookingService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
        }
        public async Task<BookingReadOnlyDTO?> GetBookingByIdAsync(int id)
        {
            
            var booking = await unitOfWork.BookingRepository.GetBookingByIdAsync(id);
            if (booking == null || booking.IsDeleted)
            {
                throw new EntityNotFoundException($"Booking with ID {id} not found.");
            }
            
            return mapper.Map<BookingReadOnlyDTO>(booking);
           
        }
        public async Task<List<BookingReadOnlyDTO>> GetBookingsByHouseIdAsync(int houseId)
        {
            var bookings = await unitOfWork.BookingRepository.GetBookingsByHouseIdAsync(houseId);
            return mapper.Map<List<BookingReadOnlyDTO>>(bookings);
        }
        public async Task<List<BookingReadOnlyDTO>> GetBookingsByRenterIdAsync(int renterId)
        {
            var bookings = await unitOfWork.BookingRepository.GetBookingsByRenterIdAsync(renterId);
            return mapper.Map<List<BookingReadOnlyDTO>>(bookings);
        }

        public async Task RegisterBookingAsync(BookingRegisterDTO dto)
        {

            if (dto.StartDate <= DateOnly.FromDateTime(DateTime.Now))
            {
                throw new InvalidBookingDatesException("Start date must be in the future.");
            }

            if (dto.EndDate <= dto.StartDate)
            {
                throw new InvalidBookingDatesException("End date must be after start date.");
            }

                House? house = await unitOfWork.HouseRepository.GetHouseByIdAsync(dto.HouseId);

            if(house == null)
            {
                throw new EntityNotFoundException($"House with ID {dto.HouseId} not found.");
            }

            Renter? renter = await unitOfWork.RenterRepository.GetRenterByIdAsync(dto.RenterId);

            if (renter == null)
            {
                throw new EntityNotFoundException($"Renter with ID {dto.RenterId} not found.");
            }

            var existingBookings = await unitOfWork.BookingRepository.GetBookingsByHouseIdAsync(dto.HouseId);

            bool hasOverlap = existingBookings.Any(b => (dto.StartDate < b.EndDate && dto.EndDate > b.StartDate));

            if (hasOverlap)
            {
                throw new BookingsOverlapException("The requested booking dates overlap with an existing booking.");
            }

            var booking = mapper.Map<Booking>(dto);
            await unitOfWork.BookingRepository.AddAsync(booking);
            await unitOfWork.SaveAsync();
            logger.LogInformation($"Registered booking for house ID {booking.HouseId} by renter ID {booking.RenterId}.");
           
        }

        public async Task DeleteBooking(BookingCancelDTO dto)
        {
            var booking = await unitOfWork.BookingRepository.GetBookingByIdAsync(dto.Id);
            if(booking == null)
            {
                throw new EntityNotFoundException("Booking not found.");
            }
            booking.IsDeleted = true;
            await unitOfWork.SaveAsync();
            logger.LogInformation($"Deleted booking with ID {booking.Id}.");
        }

    }
}
