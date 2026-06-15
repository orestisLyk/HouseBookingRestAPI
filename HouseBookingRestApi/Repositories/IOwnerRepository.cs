using HouseBookingRestApi.Core;
using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IOwnerRepository
    {
        Task<Owner?> GetOwnerByIdAsync(int ownerId);

        Task<PaginatedResult<Owner>> GetOwnersAsync(int pageNumber, int pageSize);
    }
}
