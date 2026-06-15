using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseBookingRestApi.Repositories
{
    public class HouseImageRepository : BaseRepository<HouseImage>, IHouseImageRepository
    {
        public HouseImageRepository(HouseBookingRestApiContext context) : base(context)
        {
        }
        public async Task<HouseImage?> GetHouseImageByIdAsync(int houseImageId)
        {
            return await _context.HouseImages.FirstOrDefaultAsync(h => h.Id == houseImageId);
        }
        public async Task<IEnumerable<HouseImage>> GetHouseImagesByHouseIdAsync(int houseId)
        {
            return await _context.HouseImages.Where(h => h.HouseId == houseId).ToListAsync();
        }
    }
}
