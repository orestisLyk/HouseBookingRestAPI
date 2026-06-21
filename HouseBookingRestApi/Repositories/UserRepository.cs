using HouseBookingRestApi.Core;
using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {

        public UserRepository(HouseBookingRestApiContext context) : base(context)
        {
        }

        public async Task<User?> GetUserAsync(string username, string hashedPassword)
        {
            User? user = await _context.Users.Include(u => u.Role)
                .Include(u => u.Owner)
                .Include(u => u.Renter)
                .FirstOrDefaultAsync(u => u.Username == username);

            if(user == null || user.Password != hashedPassword)
            {
                return null;
            }

            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(u => u.Role)
                .Include(u => u.Owner)
                .Include(u => u.Renter)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize)
        {
            var totalUsers = await _context.Users.CountAsync();
            var users = await _context.Users
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            return new PaginatedResult<User>(users, totalUsers, pageNumber, pageSize);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FirstAsync(u => u.Id == id && !u.IsDeleted);
            return user;
        }

        
    }
}
