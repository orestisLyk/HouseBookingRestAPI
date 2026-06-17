using AutoMapper;
using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Security;
using HouseBookingRestApi.Exceptions;

namespace HouseBookingRestApi.Service
{
    public class HouseService : IHouseService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<UserService> logger;
        private readonly IEncryptionUtil encryptionUtil;
        private IMapper mapper;
        public async Task<HouseReadOnlyDTO> CreateHouseAsync(HouseRegisterDTO dto)
        { 
            House house = mapper.Map<House>(dto);
            await unitOfWork.HouseRepository.AddAsync(house);
            await unitOfWork.SaveAsync();
            return mapper.Map<HouseReadOnlyDTO>(house);
        }

        public async Task<HouseReadOnlyDTO?> GetHouseByIdAsync(int id)
        {
            try
            {
                House? house = await unitOfWork.HouseRepository.GetHouseByIdAsync(id);
                if (house == null)
                {
                    throw new EntityNotFoundException($"House with ID {id} not found.");
                }
                return mapper.Map<HouseReadOnlyDTO>(house);

            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError(ex, $"House with ID {id} not found.");
                throw;
            }
        }

        public async Task<List<HouseReadOnlyDTO>> GetHousesByOwnerId(int ownerId)
        {
            Owner? owner = await unitOfWork.OwnerRepository.GetOwnerByIdAsync(ownerId);
            if (owner == null)
            {
                throw new EntityNotFoundException($"Owner with ID {ownerId} not found.");
            }
            List<House> houses = owner.Houses.ToList();
            return mapper.Map<List<HouseReadOnlyDTO>>(houses);
        }

        public async Task<PaginatedResult<HouseReadOnlyDTO>> GetPaginatedHousesAsync(int pageNumber, int pageSize)
        {
            var Houses = await unitOfWork.HouseRepository.GetHousesAsync(pageNumber, pageSize);

            var dtoData = mapper.Map<List<HouseReadOnlyDTO>>(Houses.Data);

            return new PaginatedResult<HouseReadOnlyDTO>(dtoData, Houses.TotalCount, pageNumber, pageSize);
        }
    }
}
