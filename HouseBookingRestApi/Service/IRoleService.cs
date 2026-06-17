using HouseBookingRestApi.Models;
using HouseBookingRestApi.DTO;

namespace HouseBookingRestApi.Service
{
    public interface IRoleService
    {
        Task<List<RoleReadOnlyDTO>> GetAllRolesAsync();
    }
}
