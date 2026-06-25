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
    public class BookingRepositoryTests
    {
        private readonly HouseBookingRestApiContext context;
        private readonly BookingRepository repository;
        private static CancellationToken ct => TestContext.Current.CancellationToken;

        public BookingRepositoryTests()
        {
            context = TestDbContextFactory.Create();
            repository = new BookingRepository(context);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithValidId_ReturnsBooking()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword",
                Firstname = "Test",
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
                Address = "123 Test St",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };
            var booking = new Booking
            {
                Id = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
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
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetBookingByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.HouseId);
            Assert.Equal(1, result.RenterId);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await repository.GetBookingByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBookingsByHouseIdAsync_ReturnsBookingsForHouse()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword",
                Firstname = "Test",
                Lastname = "User",
                RoleId = 1,
                Role = role
            };
            var owner = new Owner { Id = 1, UserId = 1, User = user };
            var renter = new Renter { Id = 1, UserId = 1, User = user };
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
            context.Renters.Add(renter);
            context.Houses.AddRange(house1, house2);

            context.Bookings.Add(new Booking
            {
                Id = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                HouseId = 1,
                House = house1,
                RenterId = 1,
                Renter = renter
            });
            context.Bookings.Add(new Booking
            {
                Id = 2,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                HouseId = 1,
                House = house1,
                RenterId = 1,
                Renter = renter
            });
            context.Bookings.Add(new Booking
            {
                Id = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12)),
                HouseId = 2,
                House = house2,
                RenterId = 1,
                Renter = renter
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetBookingsByHouseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, b => Assert.Equal(1, b.HouseId));
        }

        [Fact]
        public async Task GetBookingsByHouseIdAsync_ExcludesDeletedBookings()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword",
                Firstname = "Test",
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
                Address = "123 Test St",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Renters.Add(renter);
            context.Houses.Add(house);

            context.Bookings.Add(new Booking
            {
                Id = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter,
                IsDeleted = false
            });
            context.Bookings.Add(new Booking
            {
                Id = 2,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter,
                IsDeleted = true
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetBookingsByHouseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task GetBookingsByRenterIdAsync_ReturnsBookingsForRenter()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user1 = new User
            {
                Id = 1,
                Username = "user1",
                Email = "user1@example.com",
                Password = "hashedpassword",
                Firstname = "User",
                Lastname = "One",
                RoleId = 1,
                Role = role
            };
            var user2 = new User
            {
                Id = 2,
                Username = "user2",
                Email = "user2@example.com",
                Password = "hashedpassword",
                Firstname = "User",
                Lastname = "Two",
                RoleId = 1,
                Role = role
            };
            var owner = new Owner { Id = 1, UserId = 1, User = user1 };
            var renter1 = new Renter { Id = 1, UserId = 1, User = user1 };
            var renter2 = new Renter { Id = 2, UserId = 2, User = user2 };
            var house = new House
            {
                Id = 1,
                Name = "Test House",
                Description = "Test Description",
                Address = "123 Test St",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };

            context.Roles.Add(role);
            context.Users.AddRange(user1, user2);
            context.Owners.Add(owner);
            context.Renters.AddRange(renter1, renter2);
            context.Houses.Add(house);

            context.Bookings.Add(new Booking
            {
                Id = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter1
            });
            context.Bookings.Add(new Booking
            {
                Id = 2,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter1
            });
            context.Bookings.Add(new Booking
            {
                Id = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12)),
                HouseId = 1,
                House = house,
                RenterId = 2,
                Renter = renter2
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetBookingsByRenterIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, b => Assert.Equal(1, b.RenterId));
        }

        [Fact]
        public async Task GetBookingsByRenterIdAsync_ExcludesDeletedBookings()
        {
            // Arrange
            var role = new Role { Id = 1, Name = "User", Description = "Regular User" };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword",
                Firstname = "Test",
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
                Address = "123 Test St",
                Region = "Test Region",
                PricePerNight = 100.00m,
                OwnerId = 1,
                Owner = owner
            };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.Owners.Add(owner);
            context.Renters.Add(renter);
            context.Houses.Add(house);

            context.Bookings.Add(new Booking
            {
                Id = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter,
                IsDeleted = false
            });
            context.Bookings.Add(new Booking
            {
                Id = 2,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                HouseId = 1,
                House = house,
                RenterId = 1,
                Renter = renter,
                IsDeleted = true
            });

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetBookingsByRenterIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }
    }
}

