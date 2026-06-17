using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;

namespace HouseBookingRestApi.Service
{
    public interface IUserService
    {
        Task<User?> VerifyAndGetUserAsync(UserLoginDTO dto);

        Task<UserReadOnlyDTO?> GetUserByUsernameAsync(string username);

        Task RegisterUserAsync(UserRegisterDTO dto);

        Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersAsync(int pageNumber, int pageSize);
    }
}
