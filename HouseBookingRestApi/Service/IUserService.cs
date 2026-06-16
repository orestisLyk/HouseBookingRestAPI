using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;

namespace HouseBookingRestApi.Service
{
    public interface IUserService : IBaseRepository<User>
    {
        Task<User?> VerifyAndGetUserAsync(UserLoginDTO dto);

        Task<UserReadOnlyDTO?> GetUserByUsernameAsync(string username);

        Task<bool> RegisterUserAsync(UserRegisterDTO dto);

        Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersAsync(int pageNumber, int pageSize);
    }
}
