using HouseBookingRestApi.Controllers;
using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;
using Xunit;

namespace HouseBookingRestApi.Tests.Controllers
{
    public class HouseControllerTests
    {
        private readonly HouseController controller;
        private readonly IHouseService houseService;

        public HouseControllerTests()
        {
            houseService = Substitute.For<IHouseService>();
            controller = new HouseController(houseService);
        }

        private void SetupUserClaims(string role, int? ownerId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role)
            };
            if (ownerId.HasValue)
                claims.Add(new Claim("OwnerId", ownerId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetHouseById_WithValidId_ReturnsOk()
        {
            // Arrange
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "123 Test St", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act
            var result = await controller.GetHouseById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedHouse = Assert.IsType<HouseReadOnlyDTO>(okResult.Value);
            Assert.Equal(1, returnedHouse.Id);
        }

        [Fact]
        public async Task GetHouseById_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            houseService.GetHouseByIdAsync(999).Throws(new EntityNotFoundException("House not found"));

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.GetHouseById(999)
            );
        }

        [Fact]
        public async Task GetAllHouses_ReturnsOk()
        {
            // Arrange
            var houses = new PaginatedResult<HouseReadOnlyDTO>(
                new List<HouseReadOnlyDTO>
                {
                    new HouseReadOnlyDTO { Id = 1, OwnerId = 1, Name = "House 1", Address = "Address 1", Region = "Region 1", PricePerNight = 100.00m },
                    new HouseReadOnlyDTO { Id = 2, OwnerId = 1, Name = "House 2", Address = "Address 2", Region = "Region 2", PricePerNight = 150.00m }
                },
                2,
                1,
                10
            );
            houseService.GetPaginatedHousesAsync(1, 10).Returns(houses);

            // Act
            var result = await controller.GetAllHouses(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedHouses = Assert.IsType<PaginatedResult<HouseReadOnlyDTO>>(okResult.Value);
            Assert.Equal(2, returnedHouses.TotalCount);
        }

        [Fact]
        public async Task CreateHouse_AsOwner_ReturnsCreated()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var dto = new HouseRegisterDTO("New House", "Description", "456 New St", "New Region", 200.00m);
            var createdHouse = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "New House", 
                Address = "456 New St", 
                Region = "New Region", 
                PricePerNight = 200.00m 
            };
            houseService.CreateHouseAsync(Arg.Any<HouseRegisterDTO>()).Returns(createdHouse);

            // Act
            var result = await controller.CreateHouse(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedHouse = Assert.IsType<HouseReadOnlyDTO>(createdResult.Value);
            Assert.Equal("New House", returnedHouse.Name);
        }

        [Fact]
        public async Task CreateHouse_AsNonOwner_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter");
            var dto = new HouseRegisterDTO("New House", "Description", "456 New St", "New Region", 200.00m);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.CreateHouse(dto)
            );
        }

        [Fact]
        public async Task CreateHouse_WithoutOwnerIdClaim_ThrowsForbidden()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Owner")
                // No OwnerId claim
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            var dto = new HouseRegisterDTO("New House", "Description", "456 New St", "New Region", 200.00m);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.CreateHouse(dto)
            );
        }

        [Fact]
        public async Task GetAllOwnerHouses_ReturnsOk()
        {
            // Arrange
            var houses = new List<HouseReadOnlyDTO>
            {
                new HouseReadOnlyDTO { Id = 1, OwnerId = 1, Name = "House 1", Address = "Address 1", Region = "Region 1", PricePerNight = 100.00m },
                new HouseReadOnlyDTO { Id = 2, OwnerId = 1, Name = "House 2", Address = "Address 2", Region = "Region 2", PricePerNight = 150.00m }
            };
            houseService.GetHousesByOwnerId(1).Returns(houses);

            // Act
            var result = await controller.getAllOwnerHouses(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedHouses = Assert.IsType<List<HouseReadOnlyDTO>>(okResult.Value);
            Assert.Equal(2, returnedHouses.Count);
        }

        [Fact]
        public async Task DeleteHouse_AsAdmin_ReturnsNoContent()
        {
            // Arrange
            SetupUserClaims("Admin");
            houseService.DeleteHouseAsync(1).Returns(true);

            // Act
            var result = await controller.DeleteHouse(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteHouse_AsOwnerOfHouse_ReturnsNoContent()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "123 Test St", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            houseService.GetHouseByIdAsync(1).Returns(house);
            houseService.DeleteHouseAsync(1).Returns(true);

            // Act
            var result = await controller.DeleteHouse(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteHouse_AsOwnerOfDifferentHouse_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 2);
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "123 Test St", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.DeleteHouse(1)
            );
        }

        [Fact]
        public async Task DeleteHouse_AsOwnerWithoutOwnerIdClaim_ThrowsForbidden()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Owner")
                // No OwnerId claim
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "123 Test St", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.DeleteHouse(1)
            );
        }

        [Fact]
        public async Task DeleteHouse_AsRenter_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter");

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.DeleteHouse(1)
            );
        }

        [Fact]
        public async Task DeleteHouse_WithNonExistentHouse_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            houseService.GetHouseByIdAsync(999).Returns((HouseReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.DeleteHouse(999)
            );
        }
    }
}
