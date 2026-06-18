using AutoMapper;
using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Security;
using HouseBookingRestApi.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace HouseBookingRestApi.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<UserService> logger;
        private readonly IEncryptionUtil encryptionUtil;
        private IMapper mapper;
        private IConfiguration configuration;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IEncryptionUtil encryptionUtil, IMapper mapper, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.encryptionUtil = encryptionUtil;
            this.mapper = mapper;
            this.configuration = configuration;
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
                User? existingUser = await unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username);
                if(existingUser != null)
                {
                    throw new EntityAlreadyExistsException("A User with this username already exists");
                }

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
            catch (EntityAlreadyExistsException ex)
            {
                logger.LogError(ex, "Username already exists");
            }
        }

        public string CreateUserToken(int userId, string username, string email, Role userRole)
        {
            try
            {
                var appSecurityKey = configuration.GetValue<string>("Jwt:Key");
                if (appSecurityKey == null)
                {
                    throw new InvalidOperationException("JWT security key is not configured correctly.");
                }
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSecurityKey));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var ClaimsInfo = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, userRole.Name)
                };

                var jwtSecurityToken = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: ClaimsInfo,
                    expires: DateTime.Now.AddHours(12),
                    signingCredentials: signingCredentials
                );

                //serialize token
                var userToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                return userToken;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Failed to create user token.");
                throw;
            }
        }
    }
}
