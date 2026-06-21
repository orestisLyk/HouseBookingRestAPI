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

        public async Task<UserReadOnlyDTO> VerifyAndGetUserAsync(UserLoginDTO dto)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username);

            if (user == null || user.IsDeleted)
            {

                throw new EntityNotFoundException("No User with username " + dto.Username + " found");
            }

            bool isPasswordValid = encryptionUtil.Verify(dto.Password, user.Password);

            if (!isPasswordValid)
            {

                throw new InvalidCredentialsException("Password is invalid");
            }

            return mapper.Map<UserReadOnlyDTO>(user);
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
                if (user == null || user.IsDeleted)
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
                    throw new EntityForbiddenException("Admin creation is forbidden through the normal sign up process.");
                    break;

                case "Owner":
                    Owner owner = new Owner()
                    {
                        User = user
                    };
                    user.Owner = owner;

                    await unitOfWork.OwnerRepository.AddAsync(owner);
                    break;

                case "Renter":
                    Renter renter = new Renter()
                    {
                        User = user
                    };
                    user.Renter = renter;
                    await unitOfWork.RenterRepository.AddAsync(renter);
                    break;

                default:
                    throw new InvalidOperationException("Invalid role name: " + role.Name);
            }

            await unitOfWork.SaveAsync();
            logger.LogInformation($"Registered user: {user.Username}");
            
        }

        public async Task<JwtTokenDTO> Login(UserLoginDTO dto)
        {
            User? user = await unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username);

            if (user == null || user.IsDeleted)
            {
                throw new InvalidCredentialsException("Invalid username or password.");
            }

            bool isPasswordValid = encryptionUtil.Verify(dto.Password, user.Password);
            if (!isPasswordValid)
            {
                throw new InvalidCredentialsException("Invalid username or password.");
            }

            // Get OwnerId and RenterId if they exist
            int? ownerId = user.Owner?.Id;
            int? renterId = user.Renter?.Id;

            string token = CreateUserToken(user.Id, user.Username, user.Email, user.Role, ownerId, renterId);
            return new JwtTokenDTO(token);
        }

        public string CreateUserToken(int userId, string username, string email, Role userRole, int? ownerId = null, int? renterId = null)
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

            // Add OwnerId if user is an Owner
            if (ownerId.HasValue)
            {
                ClaimsInfo.Add(new Claim("OwnerId", ownerId.Value.ToString()));
            }

            // Add RenterId if user is a Renter
            if (renterId.HasValue)
            {
                ClaimsInfo.Add(new Claim("RenterId", renterId.Value.ToString()));
            }

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

        public async Task DeleteUserAsync(UserDeleteDTO dto)
        {
            var user = await unitOfWork.UserRepository.GetUserByIdAsync(dto.userId);
            if(user.IsDeleted || user == null)
            {
                throw new EntityNotFoundException("User not found");
            }
            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;
        }


    }
}
