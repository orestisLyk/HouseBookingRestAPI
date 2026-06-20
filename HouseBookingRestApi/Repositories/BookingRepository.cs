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
            var booking = await _context.Bookings.FindAsync(id);
            return booking;
        }
        public async Task<List<Booking>> GetBookingsByHouseIdAsync(int houseId)
        {
            return await _context.Bookings
                .Where(b => b.HouseId == houseId && !b.IsDeleted)
                .ToListAsync();
        }
        public async Task<List<Booking>> GetBookingsByRenterIdAsync(int renterId)
        {
            return await _context.Bookings
                .Where(b => b.RenterId == renterId && !b.IsDeleted)
                .ToListAsync();
        }
    }
    
    
}
