using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;

namespace HouseBookingRestApi.Service
{
    public interface IHouseService
    {
        Task<HouseReadOnlyDTO?> GetHouseByIdAsync(int id);

        Task<List<HouseReadOnlyDTO>> GetHousesByOwnerId(int ownerId);

        Task<PaginatedResult<HouseReadOnlyDTO>> GetPaginatedHousesAsync(int pageNumber, int pageSize);

        Task<HouseReadOnlyDTO> CreateHouseAsync(HouseRegisterDTO dto);

        Task<HouseReadOnlyDTO> UpdateHouseAsync(HouseUpdateDTO dto);

        Task<bool> DeleteHouseAsync(int id);
    }
}
