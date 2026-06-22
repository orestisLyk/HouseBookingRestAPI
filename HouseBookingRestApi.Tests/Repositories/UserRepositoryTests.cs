using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseBookingRestApi.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly HouseBookingRestApiContext context;
        private readonly UserRepository repository;
        private static CancellationToken ct => TestContext.Current.CancellationToken;

        public UserRepositoryTests()
        {
            context = TestDbContextFactory.Create();
            repository = new UserRepository(context);
        }

        [Fact]
        public async Task GetUserAsync_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserAsync("testuser", "hashedpassword123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            Assert.NotNull(result.Role);
        }

        [Fact]
        public async Task GetUserAsync_WithInvalidUsername_ReturnsNull()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserAsync("wronguser", "hashedpassword123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserAsync("testuser", "wrongpassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUser()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            Assert.NotNull(result.Role);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithInvalidUsername_ReturnsNull()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserByUsernameAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsCorrectPaginatedResult()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            context.Roles.Add(role);

            for (int i = 1; i <= 15; i++)
            {
                context.Users.Add(new User
                {
                    Id = i,
                    Username = $"user{i}",
                    Email = $"user{i}@example.com",
                    Password = "hashedpassword",
                    Firstname = $"First{i}",
                    Lastname = $"Last{i}",
                    RoleId = 1,
                    Role = role
                });
            }
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUsersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task GetUsersAsync_SecondPage_ReturnsRemainingUsers()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            context.Roles.Add(role);

            for (int i = 1; i <= 15; i++)
            {
                context.Users.Add(new User
                {
                    Id = i,
                    Username = $"user{i}",
                    Email = $"user{i}@example.com",
                    Password = "hashedpassword",
                    Firstname = $"First{i}",
                    Lastname = $"Last{i}",
                    RoleId = 1,
                    Role = role
                });
            }
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUsersAsync(2, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
        }

        [Fact]
        public async Task GetUsersAsync_ExcludesDeletedUsers()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            context.Roles.Add(role);

            var activeUser = new User
            {
                Id = 1,
                Username = "activeuser",
                Email = "active@example.com",
                Password = "hashedpassword",
                Firstname = "Active",
                Lastname = "User",
                RoleId = 1,
                Role = role,
                IsDeleted = false
            };

            var deletedUser = new User
            {
                Id = 2,
                Username = "deleteduser",
                Email = "deleted@example.com",
                Password = "hashedpassword",
                Firstname = "Deleted",
                Lastname = "User",
                RoleId = 1,
                Role = role,
                IsDeleted = true
            };

            context.Users.Add(activeUser);
            context.Users.Add(deletedUser);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUsersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("activeuser", result.Data[0].Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithDeletedUser_ThrowsException()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role,
                IsDeleted = true
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await repository.GetUserByIdAsync(1)
            );
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await repository.GetUserByIdAsync(999)
            );
        }


    }
}
