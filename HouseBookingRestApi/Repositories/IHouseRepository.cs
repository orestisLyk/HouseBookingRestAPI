using HouseBookingRestApi.Core;
using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IHouseRepository
    {
        Task<House?> GetHouseByIdAsync(int houseId);

        Task<IEnumerable<House>> GetHousesByOwnerId(int ownerId);

        Task<PaginatedResult<House>> GetHousesAsync(int pageNumber, int pageSize);
    }
}
