using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetRoleByIdAsync(int roleId);

        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}
