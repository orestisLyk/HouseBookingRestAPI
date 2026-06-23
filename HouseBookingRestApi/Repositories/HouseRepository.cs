using HouseBookingRestApi.Core;
using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class HouseRepository : BaseRepository<House>, IHouseRepository
    {
        public HouseRepository(HouseBookingRestApiContext context) : base(context)
        {
        }

        public async Task<House?> GetHouseByIdAsync(int houseId)
        {
            return await _context.Houses.Include(h => h.Owner)
                .Include(h => h.Bookings)
                .Include(h => h.HouseImages)
                .FirstOrDefaultAsync(h => h.Id == houseId && !h.IsDeleted);
        }

        public async Task<IEnumerable<House>> GetHousesByOwnerId(int ownerId)
        {
            return await _context.Houses
                .Include(h => h.Owner)
                .Include(h => h.Bookings)
                .Include(h => h.HouseImages)
                .Where(h => h.OwnerId == ownerId && !h.IsDeleted)
                .ToListAsync();
        }

        public async Task<PaginatedResult<House>> GetHousesAsync(int pageNumber, int pageSize)
        {
            var totalHouses = await _context.Houses.CountAsync();
            var houses = await _context.Houses
                .Include(h => h.Owner)
                .Include(h => h.Bookings)
                .Include(h => h.HouseImages)
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedResult<House>(houses, totalHouses, pageNumber, pageSize);
        }
    }
}
