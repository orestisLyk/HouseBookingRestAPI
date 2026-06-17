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
            try
            {
                var booking = await unitOfWork.BookingRepository.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    logger.LogWarning($"Booking with ID {id} not found.");
                    throw new EntityNotFoundException($"Booking with ID {id} not found.");
                }
                return mapper.Map<BookingReadOnlyDTO>(booking);
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError(ex, $"Could not find booking with ID {id}.");
                throw;
            }
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
            var booking = mapper.Map<Booking>(dto);
            await unitOfWork.BookingRepository.AddAsync(booking);
            await unitOfWork.SaveAsync();
            logger.LogInformation($"Registered booking for house ID {booking.HouseId} by renter ID {booking.RenterId}.");
        }
    }
}
