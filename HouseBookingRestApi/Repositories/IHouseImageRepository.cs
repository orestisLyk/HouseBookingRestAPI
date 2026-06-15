using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.Repositories
{
    public interface IHouseImageRepository
    {
        Task<HouseImage?> GetHouseImageByIdAsync(int houseImageId);

        Task<IEnumerable<HouseImage>> GetHouseImagesByHouseIdAsync(int houseId);
    }
}
