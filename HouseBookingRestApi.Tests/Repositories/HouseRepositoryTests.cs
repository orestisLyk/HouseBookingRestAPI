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
    public class HouseRepositoryTests
    {
        private readonly HouseBookingRestApiContext context;
        private readonly HouseRepository repository;
        private static CancellationToken ct => TestContext.Current.CancellationToken;

        public HouseRepositoryTests()
        {
            context = TestDbContextFactory.Create();
            repository = new HouseRepository(context);
        }

        [Fact]
        public async Task GetHouseByIdAsync_WithValidId_ReturnsHouseWithIncludes()
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
            var house = new House
            {
                Id = 1,
                Name = "Test House",
                Description = "A beautiful test house",
                Address = "123 Test St",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Houses.Add(house);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHouseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test House", result.Name);
            Assert.NotNull(result.Owner);
            Assert.NotNull(result.Bookings);
        }

        [Fact]
        public async Task GetHouseByIdAsync_WithDeletedHouse_ReturnsNull()
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
            var house = new House
            {
                Id = 1,
                Name = "Test House",
                Description = "A beautiful test house",
                Address = "123 Test St",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner,
                IsDeleted = true
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Houses.Add(house);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHouseByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHouseByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await repository.GetHouseByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHousesByOwnerId_ReturnsHousesForOwner()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Owner", Description = "House Owner" };
            var user1 = new User
            {
                Id = 1,
                Username = "owner1",
                Email = "owner1@example.com",
                Password = "hashedpassword",
                Firstname = "Owner",
                Lastname = "One",
                RoleId = 1,
                Role = role
            };
            var user2 = new User
            {
                Id = 2,
                Username = "owner2",
                Email = "owner2@example.com",
                Password = "hashedpassword",
                Firstname = "Owner",
                Lastname = "Two",
                RoleId = 1,
                Role = role
            };
            var owner1 = new Owner { Id = 1, UserId = 1, User = user1 };
            var owner2 = new Owner { Id = 2, UserId = 2, User = user2 };

            context.Roles.Add(role);
            context.Users.AddRange(user1, user2);
            context.Owners.AddRange(owner1, owner2);

            context.Houses.Add(new House
            {
                Id = 1,
                Name = "House 1",
                Description = "Description 1",
                Address = "Address 1",
                Region = "Region 1",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner1
            });
            context.Houses.Add(new House
            {
                Id = 2,
                Name = "House 2",
                Description = "Description 2",
                Address = "Address 2",
                Region = "Region 2",
                PricePerNight = 150.00m,
                OwnerId = 1,
                Owner = owner1
            });
            context.Houses.Add(new House
            {
                Id = 3,
                Name = "House 3",
                Description = "Description 3",
                Address = "Address 3",
                Region = "Region 3",
                PricePerNight = 200.00m,
                OwnerId = 2,
                Owner = owner2
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHousesByOwnerId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, h => Assert.Equal(1, h.OwnerId));
        }

        [Fact]
        public async Task GetHousesByOwnerId_ExcludesDeletedHouses()
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

            context.Houses.Add(new House
            {
                Id = 1,
                Name = "Active House",
                Description = "Active Description",
                Address = "Active Address",
                Region = "Active Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner,
                IsDeleted = false
            });
            context.Houses.Add(new House
            {
                Id = 2,
                Name = "Deleted House",
                Description = "Deleted Description",
                Address = "Deleted Address",
                Region = "Deleted Region",
                PricePerNight = 150.00m,
                OwnerId = 1,
                Owner = owner,
                IsDeleted = true
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHousesByOwnerId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Active House", result.First().Name);
        }

        [Fact]
        public async Task GetHousesAsync_ReturnsCorrectPaginatedResult()
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

            for (int i = 1; i <= 15; i++)
            {
                context.Houses.Add(new House
                {
                    Id = i,
                    Name = $"House {i}",
                    Description = $"Description {i}",
                    Address = $"Address {i}",
                    Region = $"Region {i}",
                    PricePerNight = 100.00m + i,
                    OwnerId = 1,
                    Owner = owner
                });
            }

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHousesAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task GetHousesAsync_SecondPage_ReturnsRemainingHouses()
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

            for (int i = 1; i <= 15; i++)
            {
                context.Houses.Add(new House
                {
                    Id = i,
                    Name = $"House {i}",
                    Description = $"Description {i}",
                    Address = $"Address {i}",
                    Region = $"Region {i}",
                    PricePerNight = 100.00m + i,
                    OwnerId = 1,
                    Owner = owner
                });
            }

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHousesAsync(2, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
        }

        [Fact]
        public async Task GetHousesAsync_ExcludesDeletedHouses()
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

            context.Houses.Add(new House
            {
                Id = 1,
                Name = "Active House",
                Description = "Active Description",
                Address = "Active Address",
                Region = "Active Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner,
                IsDeleted = false
            });
            context.Houses.Add(new House
            {
                Id = 2,
                Name = "Deleted House",
                Description = "Deleted Description",
                Address = "Deleted Address",
                Region = "Deleted Region",
                PricePerNight = 150.00m,
                OwnerId = 1,
                Owner = owner,
                IsDeleted = true
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHousesAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("Active House", result.Data[0].Name);
        }
    }
}
