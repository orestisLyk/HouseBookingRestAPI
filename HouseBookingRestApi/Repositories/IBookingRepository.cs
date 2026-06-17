using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IBookingRepository : IBaseRepository<Booking>
    {
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<List<Booking>> GetBookingsByHouseIdAsync(int houseId);
        Task<List<Booking>> GetBookingsByRenterIdAsync(int renterId);
    }
}
