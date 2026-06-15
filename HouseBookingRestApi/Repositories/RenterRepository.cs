using HouseBookingRestApi.Core;
using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class RenterRepository : BaseRepository<Renter>, IRenterRepository
    {
        public RenterRepository(HouseBookingRestApiContext context) : base(context)
        {
        }
        public async Task<Renter?> GetRenterByIdAsync(int renterId)
        {
            return await _context.Renters.Include(r => r.User)
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.Id == renterId);
        }
        public async Task<PaginatedResult<Renter>> GetRentersAsync(int pageNumber, int pageSize)
        {
            var totalRenters = await _context.Renters.CountAsync();
            var renters = await _context.Renters
                .Include(r => r.User)
                .Include(r => r.Bookings)
                .OrderBy(r => r.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedResult<Renter>(renters, totalRenters, pageNumber, pageSize);
        }
    {
    }
}
