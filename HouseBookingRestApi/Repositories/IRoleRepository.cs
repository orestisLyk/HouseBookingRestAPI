using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<Role?> GetRoleByIdAsync(int roleId);

        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}
