using HouseBookingRestApi.DTO;

namespace HouseBookingRestApi.Service
{
    public interface IHouseImageService
    {
        Task<HouseImageReadOnlyDTO> GetImageByIdAsync(int Id);

        Task<List<HouseImageReadOnlyDTO>> GetImagesByHouseId(int houseId);

        Task CreateImage
    }
}
