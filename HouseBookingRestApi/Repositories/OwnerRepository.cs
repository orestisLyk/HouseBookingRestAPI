using HouseBookingRestApi.Core;
using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class OwnerRepository : BaseRepository<Owner>, IOwnerRepository
    {
        public OwnerRepository(HouseBookingRestApiContext context) : base(context)
        {
        }
        public async Task<Owner?> GetOwnerByIdAsync(int ownerId)
        {
            return await _context.Owners.Include(o => o.User)
                .Include(o => o.Houses)
                .FirstOrDefaultAsync(o => o.Id == ownerId);
        }
        public async Task<PaginatedResult<Owner>> GetOwnersAsync(int pageNumber, int pageSize)
        {
            var totalOwners = await _context.Owners.CountAsync();
            var owners = await _context.Owners
                .Include(o => o.User)
                .Include(o => o.Houses)
                .OrderBy(o => o.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedResult<Owner>(owners, totalOwners, pageNumber, pageSize);
        }
    }
}
