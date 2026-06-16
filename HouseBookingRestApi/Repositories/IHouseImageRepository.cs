using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IHouseImageRepository : IBaseRepository<HouseImage>
    {
        Task<HouseImage?> GetHouseImageByIdAsync(int houseImageId);

        Task<IEnumerable<HouseImage>> GetHouseImagesByHouseIdAsync(int houseId);
    }
}
