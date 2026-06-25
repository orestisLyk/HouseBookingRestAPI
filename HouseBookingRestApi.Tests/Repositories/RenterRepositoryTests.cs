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
    public class RenterRepositoryTests
    {
        private readonly HouseBookingRestApiContext context;
        private readonly RenterRepository repository;
        private static CancellationToken ct => TestContext.Current.CancellationToken;

        public RenterRepositoryTests()
        {
            context = TestDbContextFactory.Create();
            repository = new RenterRepository(context);
        }

        [Fact]
        public async Task GetRenterByIdAsync_WithValidId_ReturnsRenterWithIncludes()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Renter", Description = "House Renter" };
            var user = new User
            {
                Id = 1,
                Username = "renteruser",
                Email = "renter@example.com",
                Password = "hashedpassword",
                Firstname = "Renter",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };
            var renter = new Renter { Id = 1, UserId = 1, User = user };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Renters.Add(renter);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetRenterByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.UserId);
            Assert.NotNull(result.User);
            Assert.Equal("renteruser", result.User.Username);
            Assert.NotNull(result.Bookings);
        }

        [Fact]
        public async Task GetRenterByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await repository.GetRenterByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRentersAsync_ReturnsCorrectPaginatedResult()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Renter", Description = "House Renter" };
            context.Roles.Add(role);

            for (int i = 1; i <= 15; i++)
            {
                var user = new User
                {
                    Id = i,
                    Username = $"renter{i}",
                    Email = $"renter{i}@example.com",
                    Password = "hashedpassword",
                    Firstname = $"Renter{i}",
                    Lastname = "User",
                    RoleId = 1,
                    Role = role
                };
                context.Users.Add(user);
                context.Renters.Add(new Renter { Id = i, UserId = i, User = user });
            }

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetRentersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task GetRentersAsync_SecondPage_ReturnsRemainingRenters()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Renter", Description = "House Renter" };
            context.Roles.Add(role);

            for (int i = 1; i <= 15; i++)
            {
                var user = new User
                {
                    Id = i,
                    Username = $"renter{i}",
                    Email = $"renter{i}@example.com",
                    Password = "hashedpassword",
                    Firstname = $"Renter{i}",
                    Lastname = "User",
                    RoleId = 1,
                    Role = role
                };
                context.Users.Add(user);
                context.Renters.Add(new Renter { Id = i, UserId = i, User = user });
            }

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetRentersAsync(2, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
        }

        [Fact]
        public async Task GetRentersAsync_WithNoRenters_ReturnsEmptyPaginatedResult()
        {
            // Act
            var result = await repository.GetRentersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetRenterByIdAsync_IncludesBookings()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "Renter", Description = "House Renter" };
            var user = new User
            {
                Id = 1,
                Username = "renteruser",
                Email = "renter@example.com",
                Password = "hashedpassword",
                Firstname = "Renter",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };
            var owner = new Owner { Id = 1, UserId = 1, User = user };
            var renter = new Renter { Id = 1, UserId = 1, User = user };
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
            var booking1 = new Booking
            {
                Id = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter
            };
            var booking2 = new Booking
            {
                Id = 2,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Renters.Add(renter);
            context.Houses.Add(house);
            context.Bookings.AddRange(booking1, booking2);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetRenterByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Bookings);
            Assert.Equal(2, result.Bookings.Count);
        }
    }
}

