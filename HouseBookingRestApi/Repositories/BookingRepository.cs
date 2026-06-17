using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        public BookingRepository(HouseBookingRestApiContext context) : base(context)
        {
        }
        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings.FindAsync(id);
        }
        public async Task<List<Booking>> GetBookingsByHouseIdAsync(int houseId)
        {
            return await _context.Bookings
                .Where(b => b.HouseId == houseId)
                .ToListAsync();
        }
        public async Task<List<Booking>> GetBookingsByRenterIdAsync(int renterId)
        {
            return await _context.Bookings
                .Where(b => b.RenterId == renterId)
                .ToListAsync();
        }
    }
    
    
}
