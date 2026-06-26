using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface ICapabilityRepository : IBaseRepository<Capability>
    {
        Task<IEnumerable<Capability>> GetCapabilitiesAsync();

        Task<IEnumerable<Capability>> GetCapabilitiesByRoleIdAsync(int roleId);
    }
}
