using AutoMapper;
using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Security;
using HouseBookingRestApi.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HouseBookingRestApi.Tests.Services
{
    public class UserServiceTests
    {
        private readonly UserService service;
        private readonly IUnitOfWork unitOfWork;
        private IMapper mapper;
        private readonly IEncryptionUtil encryptionUtil;
        private readonly ILogger<UserService> logger;
        private readonly IConfiguration configuration;

        public UserServiceTests()
        {
            unitOfWork = Substitute.For<IUnitOfWork>();
            mapper = Substitute.For<IMapper>();
            encryptionUtil = Substitute.For<IEncryptionUtil>();
            logger = Substitute.For<ILogger<UserService>>();
            configuration = Substitute.For<IConfiguration>();
            service = new UserService(unitOfWork, logger, encryptionUtil, mapper, configuration);
        }

        [Fact]
        public async Task VerifyAndGetUserAsync_WithValidCredentials_ReturnsUserDTO()
        {
            // Arrange
            var dto = new UserLoginDTO("testuser", "password123");
            var role = new Role { Id = 1, Name = "User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role,
                IsDeleted = false
            };
            var userDTO = new UserReadOnlyDTO(1, "testuser", "test@example.com", "Test", "User", "User");

            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);
            encryptionUtil.Verify("password123", "hashedpassword").Returns(true);
            mapper.Map<UserReadOnlyDTO>(user).Returns(userDTO);

            // Act
            var result = await service.VerifyAndGetUserAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task VerifyAndGetUserAsync_WithNonExistentUser_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new UserLoginDTO("nonexistent", "password123");
            unitOfWork.UserRepository.GetUserByUsernameAsync("nonexistent").Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.VerifyAndGetUserAsync(dto)
            );
        }

        [Fact]
        public async Task VerifyAndGetUserAsync_WithDeletedUser_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new UserLoginDTO("testuser", "password123");
            var user = new User { Id = 1, Username = "testuser", IsDeleted = true };
            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.VerifyAndGetUserAsync(dto)
            );
        }

        [Fact]
        public async Task VerifyAndGetUserAsync_WithInvalidPassword_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var dto = new UserLoginDTO("testuser", "wrongpassword");
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "hashedpassword",
                IsDeleted = false
            };

            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);
            encryptionUtil.Verify("wrongpassword", "hashedpassword").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await service.VerifyAndGetUserAsync(dto)
            );
        }

        [Fact]
        public async Task GetPaginatedUsersAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1" },
                new User { Id = 2, Username = "user2" }
            };
            var paginatedUsers = new PaginatedResult<User>(users, 10, 1, 5);
            var userDTOs = new List<UserReadOnlyDTO>
            {
                new UserReadOnlyDTO(1, "user1", "user1@example.com", "First1", "Last1", "User"),
                new UserReadOnlyDTO(2, "user2", "user2@example.com", "First2", "Last2", "User")
            };

            unitOfWork.UserRepository.GetUsersAsync(1, 5).Returns(paginatedUsers);
            mapper.Map<List<UserReadOnlyDTO>>(users).Returns(userDTOs);

            // Act
            var result = await service.GetPaginatedUsersAsync(1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(10, result.TotalCount);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUserDTO()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Role = role,
                IsDeleted = false
            };
            var userDTO = new UserReadOnlyDTO(1, "testuser", "test@example.com", "Test", "User", "User");

            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);
            mapper.Map<UserReadOnlyDTO>(user).Returns(userDTO);

            // Act
            var result = await service.GetUserByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithNonExistentUser_ThrowsEntityNotFoundException()
        {
            // Arrange
            unitOfWork.UserRepository.GetUserByUsernameAsync("nonexistent").Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.GetUserByUsernameAsync("nonexistent")
            );
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithDeletedUser_ThrowsEntityNotFoundException()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser", IsDeleted = true };
            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.GetUserByUsernameAsync("testuser")
            );
        }

        [Fact]
        public async Task RegisterUserAsync_WithValidOwnerData_CreatesUserAndOwner()
        {
            // Arrange
            var dto = new UserRegisterDTO("newuser", "password123", "new@example.com", "New", "User", 2);
            var user = new User { Id = 1, Username = "newuser", Email = "new@example.com" };
            var role = new Role { Id = 2, Name = "Owner" };

            unitOfWork.UserRepository.GetUserByUsernameAsync("newuser").Returns((User?)null);
            mapper.Map<User>(dto).Returns(user);
            encryptionUtil.Encrypt("password123").Returns("hashedpassword");
            unitOfWork.RoleRepository.GetRoleByIdAsync(2).Returns(role);

            // Act
            await service.RegisterUserAsync(dto);

            // Assert
            await unitOfWork.UserRepository.Received(1).AddAsync(Arg.Any<User>());
            await unitOfWork.OwnerRepository.Received(1).AddAsync(Arg.Any<Owner>());
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task RegisterUserAsync_WithValidRenterData_CreatesUserAndRenter()
        {
            // Arrange
            var dto = new UserRegisterDTO("newuser", "password123", "new@example.com", "New", "User", 3);
            var user = new User { Id = 1, Username = "newuser", Email = "new@example.com" };
            var role = new Role { Id = 3, Name = "Renter" };

            unitOfWork.UserRepository.GetUserByUsernameAsync("newuser").Returns((User?)null);
            mapper.Map<User>(dto).Returns(user);
            encryptionUtil.Encrypt("password123").Returns("hashedpassword");
            unitOfWork.RoleRepository.GetRoleByIdAsync(3).Returns(role);

            // Act
            await service.RegisterUserAsync(dto);

            // Assert
            await unitOfWork.UserRepository.Received(1).AddAsync(Arg.Any<User>());
            await unitOfWork.RenterRepository.Received(1).AddAsync(Arg.Any<Renter>());
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task RegisterUserAsync_WithExistingUsername_ThrowsEntityAlreadyExistsException()
        {
            // Arrange
            var dto = new UserRegisterDTO("existinguser", "password123", "new@example.com", "New", "User", 2);
            var existingUser = new User { Id = 1, Username = "existinguser" };

            unitOfWork.UserRepository.GetUserByUsernameAsync("existinguser").Returns(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                async () => await service.RegisterUserAsync(dto)
            );
        }

        [Fact]
        public async Task RegisterUserAsync_WithAdminRole_ThrowsEntityForbiddenException()
        {
            // Arrange
            var dto = new UserRegisterDTO("newuser", "password123", "new@example.com", "New", "User", 1);
            var user = new User { Id = 1, Username = "newuser" };
            var adminRole = new Role { Id = 1, Name = "Admin" };

            unitOfWork.UserRepository.GetUserByUsernameAsync("newuser").Returns((User?)null);
            mapper.Map<User>(dto).Returns(user);
            encryptionUtil.Encrypt("password123").Returns("hashedpassword");
            unitOfWork.RoleRepository.GetRoleByIdAsync(1).Returns(adminRole);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await service.RegisterUserAsync(dto)
            );
        }

        [Fact]
        public async Task RegisterUserAsync_WithInvalidRoleId_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new UserRegisterDTO("newuser", "password123", "new@example.com", "New", "User", 999);
            var user = new User { Id = 1, Username = "newuser" };

            unitOfWork.UserRepository.GetUserByUsernameAsync("newuser").Returns((User?)null);
            mapper.Map<User>(dto).Returns(user);
            encryptionUtil.Encrypt("password123").Returns("hashedpassword");
            unitOfWork.RoleRepository.GetRoleByIdAsync(999).Returns((Role?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.RegisterUserAsync(dto)
            );
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var dto = new UserLoginDTO("testuser", "password123");
            var role = new Role { Id = 1, Name = "User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword",
                Role = role,
                IsDeleted = false
            };

            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);
            encryptionUtil.Verify("password123", "hashedpassword").Returns(true);

            // Mock configuration using GetSection approach for better compatibility
            var jwtSection = Substitute.For<IConfigurationSection>();
            jwtSection.Value.Returns("ThisIsAVerySecureSecretKeyForJWT12345678901234567890");
            configuration.GetSection("Jwt:Key").Returns(jwtSection);
            configuration["Jwt:Issuer"].Returns("TestIssuer");
            configuration["Jwt:Audience"].Returns("TestAudience");

            // Act
            var result = await service.Login(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.token);
            Assert.NotEmpty(result.token);
        }

        [Fact]
        public async Task Login_WithInvalidUsername_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var dto = new UserLoginDTO("nonexistent", "password123");
            unitOfWork.UserRepository.GetUserByUsernameAsync("nonexistent").Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await service.Login(dto)
            );
        }

        [Fact]
        public async Task Login_WithDeletedUser_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var dto = new UserLoginDTO("testuser", "password123");
            var user = new User { Id = 1, Username = "testuser", IsDeleted = true };
            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await service.Login(dto)
            );
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var dto = new UserLoginDTO("testuser", "wrongpassword");
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "hashedpassword",
                IsDeleted = false
            };

            unitOfWork.UserRepository.GetUserByUsernameAsync("testuser").Returns(user);
            encryptionUtil.Verify("wrongpassword", "hashedpassword").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await service.Login(dto)
            );
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidId_MarksUserAsDeleted()
        {
            // Arrange
            var dto = new UserDeleteDTO(1);
            var user = new User { Id = 1, Username = "testuser", IsDeleted = false };

            unitOfWork.UserRepository.GetUserByIdAsync(1).Returns(user);

            // Act
            await service.DeleteUserAsync(dto);

            // Assert
            Assert.True(user.IsDeleted);
            Assert.NotNull(user.DeletedAt);
        }

        [Fact]
        public async Task DeleteUserAsync_WithNonExistentUser_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new UserDeleteDTO(999);
            unitOfWork.UserRepository.GetUserByIdAsync(999).Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.DeleteUserAsync(dto)
            );
        }

        [Fact]
        public async Task DeleteUserAsync_WithAlreadyDeletedUser_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new UserDeleteDTO(1);
            var user = new User { Id = 1, Username = "testuser", IsDeleted = true };

            unitOfWork.UserRepository.GetUserByIdAsync(1).Returns(user);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.DeleteUserAsync(dto)
            );
        }
    }
}
