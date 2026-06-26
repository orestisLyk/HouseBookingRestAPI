using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class CapabilityRepository : BaseRepository<Capability> , ICapabilityRepository
    {
        public CapabilityRepository(HouseBookingRestApiContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Capability>> GetCapabilitiesAsync()
        {
            return await _context.Capabilities.ToListAsync();
        }

        public async Task<IEnumerable<Capability>> GetCapabilitiesByRoleIdAsync(int roleId)
        {
            return await _context.Roles
                .Where(r => r.Id == roleId)
                .SelectMany(r => r.Capabilities)
                .ToListAsync();
        }
    }
}
