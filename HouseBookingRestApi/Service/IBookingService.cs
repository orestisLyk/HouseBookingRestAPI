using HouseBookingRestApi.DTO;

namespace HouseBookingRestApi.Service
{
    public interface IBookingService
    {
        Task<BookingReadOnlyDTO?> GetBookingByIdAsync(int id);

        Task<List<BookingReadOnlyDTO>> GetBookingsByHouseIdAsync(int houseId);

        Task<List<BookingReadOnlyDTO>> GetBookingsByRenterIdAsync(int renterId);

        Task RegisterBookingAsync(BookingRegisterDTO dto);
    }
}
