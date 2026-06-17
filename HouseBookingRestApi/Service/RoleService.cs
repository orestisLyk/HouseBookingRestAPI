using AutoMapper;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Repositories;

namespace HouseBookingRestApi.Service
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<RoleService> logger;
        public RoleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RoleService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
        }
        public async Task<List<RoleReadOnlyDTO>> GetAllRolesAsync()
        {
            var roles = await unitOfWork.RoleRepository.GetAllRolesAsync();
            logger.LogInformation($"Retrieved roles from the database.");
            return mapper.Map<List<RoleReadOnlyDTO>>(roles);
        }
    }
}
