using HouseBookingRestApi.Data;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HouseBookingRestApi.Tests.Repositories
{
    public class RoleRepositoryTests
    {
        private readonly HouseBookingRestApiContext context;
        private readonly RoleRepository repository;
        private static CancellationToken ct => TestContext.Current.CancellationToken;

        public RoleRepositoryTests()
        {
            context = TestDbContextFactory.Create();
            repository = new RoleRepository(context);
        }

        [Fact]
        public async Task GetRoleByIdAsync_WithValidId_ReturnsRole()
        {
            // Arrange
            var role = new Role
            {
                Id = 1,
                Name = "Admin",
                Description = "Administrator role"
            };

            context.Roles.Add(role);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetRoleByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Admin", result.Name);
            Assert.Equal("Administrator role", result.Description);
        }

        [Fact]
        public async Task GetRoleByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await repository.GetRoleByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsAllRoles()
        {
            // Arrange
            var roles = new[]
            {
                new Role { Id = 1, Name = "Admin", Description = "Administrator role" },
                new Role { Id = 2, Name = "User", Description = "Regular user role" },
                new Role { Id = 3, Name = "Owner", Description = "House owner role" },
                new Role { Id = 4, Name = "Renter", Description = "House renter role" }
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllRolesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
            Assert.Contains(result, r => r.Name == "Admin");
            Assert.Contains(result, r => r.Name == "User");
            Assert.Contains(result, r => r.Name == "Owner");
            Assert.Contains(result, r => r.Name == "Renter");
        }

        [Fact]
        public async Task GetAllRolesAsync_WithNoRoles_ReturnsEmptyCollection()
        {
            // Act
            var result = await repository.GetAllRolesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
