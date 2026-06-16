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
             
                    throw new EntityNotFoundException("No User with username "+ dto.Username + " found"); 
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

            } catch (InvalidCredentialsException ex)
            {
                logger.LogError("Invalid password");
                throw;
            }
        }

        public async Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
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
                return dto;
            } catch (EntityNotFoundException ex)
            {
                logger.LogError(ex, "An error occurred while retrieving user by username.");
                throw;
            }
        }

        public async Task<bool> RegisterUserAsync(UserRegisterDTO dto)
        {
            User user = mapper.Map<User>(dto);
            await unitOfWork.UserRepository
            
        }
    }
}
