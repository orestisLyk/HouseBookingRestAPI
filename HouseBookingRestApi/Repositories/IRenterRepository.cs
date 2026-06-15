using HouseBookingRestApi.Core;
using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IRenterRepository
    {
        Task<Renter?> GetRenterByIdAsync(int renterId);

        Task<PaginatedResult<Renter>> GetRentersAsync(int pageNumber, int pageSize);
    }
}
