using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;

namespace HouseBookingRestApi.Service
{
    public interface IUserService
    {
        Task<UserReadOnlyDTO> VerifyAndGetUserAsync(UserLoginDTO dto);

        Task<UserReadOnlyDTO?> GetUserByUsernameAsync(string username);

        Task RegisterUserAsync(UserRegisterDTO dto);

        Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersAsync(int pageNumber, int pageSize);

        Task<JwtTokenDTO> Login(UserLoginDTO dto);

        Task DeleteUserAsync(UserDeleteDTO dto);

        Task CreateAdminAsync();
    }
}
