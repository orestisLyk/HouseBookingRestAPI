using HouseBookingRestApi.Core;
using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly EncryptionUtil _encryptionUtil;

        public UserRepository(HouseBookingRestApiContext context, EncryptionUtil encryptionUtil) : base(context)
        {
            _encryptionUtil = encryptionUtil;
        }

        public async Task<User?> GetUserAsync(string username, string hashedPassword)
        {
            User? user = await _context.Users.Include(u => u.Role)
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
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize)
        {
            var totalUsers = await _context.Users.CountAsync();
            var users = await _context.Users
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<User>(users, totalUsers, pageNumber, pageSize);
        }
    }
}
