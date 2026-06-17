using AutoMapper;
using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Security;
using HouseBookingRestApi.Exceptions;

namespace HouseBookingRestApi.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<UserService> logger;
        private readonly IEncryptionUtil encryptionUtil;
        private IMapper mapper;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IEncryptionUtil encryptionUtil, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.encryptionUtil = encryptionUtil;
            this.mapper = mapper;
        }

        public async Task<User?> VerifyAndGetUserAsync(UserLoginDTO dto)
        {
            try
            {
                var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username);

                if (user == null)
                {

                    throw new EntityNotFoundException("No User with username " + dto.Username + " found");
                }

                bool isPasswordValid = encryptionUtil.Verify(dto.Password, user.Password);

                if (!isPasswordValid)
                {

                    throw new InvalidCredentialsException("Password is invalid");
                }

                return user;
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError(ex, "Failed to load User with the given username");
                throw;

            }
            catch (InvalidCredentialsException ex)
            {
                logger.LogError("Invalid password");
                throw;
            }
        }

        public async Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersAsync(int pageNumber, int pageSize)
        {
            var result = await unitOfWork.UserRepository.GetUsersAsync(pageNumber, pageSize);
            var dtoData = mapper.Map<List<UserReadOnlyDTO>>(result.Data);

            logger.LogInformation("Retrieved {Count} users", dtoData.Count);
            return new PaginatedResult<UserReadOnlyDTO>(dtoData, result.TotalCount, result.TotalPages, result.PageSize);
        }

        public async Task<UserReadOnlyDTO?> GetUserByUsernameAsync(string username)
        {
            try
            {
                User? user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    throw new EntityNotFoundException($"Username {username} is not valid.");
                }
                UserReadOnlyDTO dto = mapper.Map<UserReadOnlyDTO>(user);
                logger.LogInformation($"User: {dto.Username} succesfully returned");
                return dto;
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError(ex, "Could not find user with username " + username);
                throw;
            }
        }

        public async Task RegisterUserAsync(UserRegisterDTO dto)
        {
            try
            {
                User user = mapper.Map<User>(dto);

                user.Password = encryptionUtil.Encrypt(dto.Password);

                await unitOfWork.UserRepository.AddAsync(user);

                Role? role = await unitOfWork.RoleRepository.GetRoleByIdAsync(dto.RoleId);
                if (role == null)
                {
                    throw new EntityNotFoundException("Role with ID " + dto.RoleId + " not found.");
                }
                switch (role.Name)
                {
                    case "Admin":
                        break;

                    case "Owner":
                        Owner owner = new Owner()
                        {
                            User = user
                        };
                        await unitOfWork.OwnerRepository.AddAsync(owner);
                        break;

                    case "Renter":
                        Renter renter = new Renter()
                        {
                            User = user
                        };
                        await unitOfWork.RenterRepository.AddAsync(renter);
                        break;

                    default:
                        throw new InvalidOperationException("Invalid role name: " + role.Name);
                }

                await unitOfWork.SaveAsync();
                logger.LogInformation($"Registered user: {user.Username}");
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError(ex, "Failed to retrieve role with ID " + dto.RoleId);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Invalid role name: " + ex.Message);
                throw;
            }
        }
    }
}
