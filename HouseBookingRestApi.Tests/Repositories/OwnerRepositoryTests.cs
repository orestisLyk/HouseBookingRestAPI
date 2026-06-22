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
    public class OwnerRepositoryTests
    {
        private readonly HouseBookingRestApiContext context;
        private readonly OwnerRepository repository;
        private static CancellationToken ct => TestContext.Current.CancellationToken;

        public OwnerRepositoryTests()
        {
            context = TestDbContextFactory.Create();
            repository = new OwnerRepository(context);
        }

        [Fact]
        public async Task GetOwnerByIdAsync_WithValidId_ReturnsOwnerWithIncludes()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Owner", Description = "House Owner" };
            var user = new User
            {
                Id = 1,
                Username = "owneruser",
                Email = "owner@example.com",
                Password = "hashedpassword",
                Firstname = "Owner",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };
            var owner = new Owner { Id = 1, UserId = 1, User = user };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetOwnerByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.UserId);
            Assert.NotNull(result.User);
            Assert.Equal("owneruser", result.User.Username);
            Assert.NotNull(result.Houses);
        }

        [Fact]
        public async Task GetOwnerByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await repository.GetOwnerByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOwnersAsync_ReturnsCorrectPaginatedResult()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Owner", Description = "House Owner" };
            context.Roles.Add(role);

            for (int i = 1; i <= 15; i++)
            {
                var user = new User
                {
                    Id = i,
                    Username = $"owner{i}",
                    Email = $"owner{i}@example.com",
                    Password = "hashedpassword",
                    Firstname = $"Owner{i}",
                    Lastname = "User",
                    RoleId = 1,
                    Role = role
                };
                context.Users.Add(user);
                context.Owners.Add(new Owner { Id = i, UserId = i, User = user });
            }

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetOwnersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task GetOwnersAsync_SecondPage_ReturnsRemainingOwners()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Owner", Description = "House Owner" };
            context.Roles.Add(role);

            for (int i = 1; i <= 15; i++)
            {
                var user = new User
                {
                    Id = i,
                    Username = $"owner{i}",
                    Email = $"owner{i}@example.com",
                    Password = "hashedpassword",
                    Firstname = $"Owner{i}",
                    Lastname = "User",
                    RoleId = 1,
                    Role = role
                };
                context.Users.Add(user);
                context.Owners.Add(new Owner { Id = i, UserId = i, User = user });
            }

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetOwnersAsync(2, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
        }

        [Fact]
        public async Task GetOwnersAsync_WithNoOwners_ReturnsEmptyPaginatedResult()
        {
            // Act
            var result = await repository.GetOwnersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetOwnerByIdAsync_IncludesHouses()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Owner", Description = "House Owner" };
            var user = new User
            {
                Id = 1,
                Username = "owneruser",
                Email = "owner@example.com",
                Password = "hashedpassword",
                Firstname = "Owner",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };
            var owner = new Owner { Id = 1, UserId = 1, User = user };
            var house1 = new House
            {
                Id = 1,
                Name = "House 1",
                Description = "Description 1",
                Address = "Address 1",
                Region = "Region 1",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };
            var house2 = new House
            {
                Id = 2,
                Name = "House 2",
                Description = "Description 2",
                Address = "Address 2",
                Region = "Region 2",
                PricePerNight = 150.00m,
                OwnerId = 1,
                Owner = owner
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Houses.AddRange(house1, house2);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetOwnerByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Houses);
            Assert.Equal(2, result.Houses.Count);
        }
    }
}
