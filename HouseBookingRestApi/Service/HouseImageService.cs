using AutoMapper;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;

namespace HouseBookingRestApi.Service
{
    public class HouseImageService : IHouseImageService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IImageStorageService imageStorageService;
        private readonly ILogger<HouseImageService> logger;
        private readonly IMapper mapper;

        public HouseImageService(IUnitOfWork unitOfWork, IImageStorageService imageStorageService, ILogger<HouseImageService> logger, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.imageStorageService = imageStorageService;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task CreateImageAsync(HouseImageCreateDTO dto)
        {
            try
            {
                string url = await imageStorageService.UploadImageAsync(dto.HouseId, dto.File);

                var houseImage = new HouseImage
                {
                    HouseId = dto.HouseId,
                    Url = url
                };

                await unitOfWork.HouseImageRepository.AddAsync(houseImage);
                await unitOfWork.SaveAsync();
            }
            catch (ImageUploadException ex)
            {
                logger.LogError(ex, "Error uploading image for house ID {HouseId}", dto.HouseId);
                throw;
            }
            catch(ArgumentException ex)
            {
                logger.LogError(ex, "Invalid argument provided for house ID {HouseId}", dto.HouseId);
                throw;
            }

        }

        public async Task<IEnumerable<HouseImageReadOnlyDTO>> GetImagesByHouseIdAsync(int houseId)
        {
            var houseImages = await unitOfWork.HouseImageRepository.GetHouseImagesByHouseIdAsync(houseId);
            return mapper.Map<List<HouseImageReadOnlyDTO>>(houseImages);
        }

        public async Task<HouseImageReadOnlyDTO> GetImageByIdAsync(int Id)
        {
            try
            {
                var houseImage = await unitOfWork.HouseImageRepository.GetHouseImageByIdAsync(Id);
                if (houseImage == null)
                {
                    throw new EntityNotFoundException($"House image with ID {Id} not found.");
                }
                return mapper.Map<HouseImageReadOnlyDTO>(houseImage);
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError(ex, "House image with ID {Id} not found.", Id);
                throw;
            }

        }
    }
}
