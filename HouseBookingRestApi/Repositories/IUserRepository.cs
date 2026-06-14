using HouseBookingRestApi.Core;
using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserAsync(string username, string hashedPassword);

        Task<User?> GetUserByUsernameAsync(string username);

        Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize);
    }
}
