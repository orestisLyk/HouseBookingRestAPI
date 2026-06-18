using HouseBookingRestApi.DTO;

namespace HouseBookingRestApi.Service
{
    public interface IHouseImageService
    {
        Task<HouseImageReadOnlyDTO> GetImageByIdAsync(int Id);

        Task<IEnumerable<HouseImageReadOnlyDTO>> GetImagesByHouseIdAsync(int houseId);

        Task CreateImageAsync(HouseImageCreateDTO dto);
    }
}
