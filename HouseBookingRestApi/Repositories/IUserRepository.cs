using HouseBookingRestApi.Core;
using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserAsync(string username, string hashedPassword);

        Task<User> GetUserByIdAsync(int id);

        Task<User?> GetUserByUsernameAsync(string username);

        Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize);
    }
}
