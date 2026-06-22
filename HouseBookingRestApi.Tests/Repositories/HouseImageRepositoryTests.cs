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
    public class HouseImageRepositoryTests
    {
        private readonly HouseBookingRestApiContext context;
        private readonly HouseImageRepository repository;
        private static CancellationToken ct => TestContext.Current.CancellationToken;

        public HouseImageRepositoryTests()
        {
            context = TestDbContextFactory.Create();
            repository = new HouseImageRepository(context);
        }

        [Fact]
        public async Task GetHouseImageByIdAsync_WithValidId_ReturnsHouseImage()
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
                Description = "Test Description",
                Address = "Test Address",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };
            var houseImage = new HouseImage
            {
                Id = 1,
                Url = "https://example.com/image1.jpg",
                HouseId = 1,
                House = house
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Houses.Add(house);
            context.HouseImages.Add(houseImage);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHouseImageByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("https://example.com/image1.jpg", result.Url);
            Assert.Equal(1, result.HouseId);
        }

        [Fact]
        public async Task GetHouseImageByIdAsync_WithDeletedImage_ReturnsNull()
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
                Description = "Test Description",
                Address = "Test Address",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };
            var houseImage = new HouseImage
            {
                Id = 1,
                Url = "https://example.com/image1.jpg",
                HouseId = 1,
                House = house,
                IsDeleted = true
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Houses.Add(house);
            context.HouseImages.Add(houseImage);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHouseImageByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHouseImageByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await repository.GetHouseImageByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHouseImagesByHouseIdAsync_ReturnsImagesForHouse()
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

            context.HouseImages.Add(new HouseImage
            {
                Id = 1,
                Url = "https://example.com/house1-image1.jpg",
                HouseId = 1,
                House = house1
            });
            context.HouseImages.Add(new HouseImage
            {
                Id = 2,
                Url = "https://example.com/house1-image2.jpg",
                HouseId = 1,
                House = house1
            });
            context.HouseImages.Add(new HouseImage
            {
                Id = 3,
                Url = "https://example.com/house2-image1.jpg",
                HouseId = 2,
                House = house2
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHouseImagesByHouseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, img => Assert.Equal(1, img.HouseId));
        }

        [Fact]
        public async Task GetHouseImagesByHouseIdAsync_ExcludesDeletedImages()
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
                Description = "Test Description",
                Address = "Test Address",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Houses.Add(house);

            context.HouseImages.Add(new HouseImage
            {
                Id = 1,
                Url = "https://example.com/active-image.jpg",
                HouseId = 1,
                House = house,
                IsDeleted = false
            });
            context.HouseImages.Add(new HouseImage
            {
                Id = 2,
                Url = "https://example.com/deleted-image.jpg",
                HouseId = 1,
                House = house,
                IsDeleted = true
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetHouseImagesByHouseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("https://example.com/active-image.jpg", result.First().Url);
        }

        [Fact]
        public async Task GetHouseImagesByHouseIdAsync_WithNoImages_ReturnsEmptyCollection()
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
                Description = "Test Description",
                Address = "Test Address",
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
            var result = await repository.GetHouseImagesByHouseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
