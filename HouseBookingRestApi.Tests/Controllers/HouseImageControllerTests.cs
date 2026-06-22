using HouseBookingRestApi.Controllers;
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
    public class HouseImageControllerTests
    {
        private readonly HouseImageController controller;
        private readonly IHouseImageService imageService;
        private readonly IHouseService houseService;

        public HouseImageControllerTests()
        {
            imageService = Substitute.For<IHouseImageService>();
            houseService = Substitute.For<IHouseService>();
            controller = new HouseImageController(imageService, houseService);
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
        public async Task CreateImage_AsOwnerOfHouse_ReturnsCreated()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var formFile = Substitute.For<IFormFile>();
            var dto = new HouseImageCreateDTO(1, formFile);
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            houseService.GetHouseByIdAsync(1).Returns(house);
            imageService.CreateImageAsync(dto).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CreateImage(dto);

            // Assert
            Assert.IsType<CreatedResult>(result);
        }

        [Fact]
        public async Task CreateImage_AsNonOwner_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter");
            var formFile = Substitute.For<IFormFile>();
            var dto = new HouseImageCreateDTO(1, formFile);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.CreateImage(dto)
            );
        }

        [Fact]
        public async Task CreateImage_AsOwnerOfDifferentHouse_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 2);
            var formFile = Substitute.For<IFormFile>();
            var dto = new HouseImageCreateDTO(1, formFile);
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.CreateImage(dto)
            );
        }

        [Fact]
        public async Task CreateImage_WithNonExistentHouse_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var formFile = Substitute.For<IFormFile>();
            var dto = new HouseImageCreateDTO(999, formFile);
            houseService.GetHouseByIdAsync(999).Returns((HouseReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.CreateImage(dto)
            );
        }

        [Fact]
        public async Task GetImagesByHouseId_ReturnsOk()
        {
            // Arrange
            var images = new List<HouseImageReadOnlyDTO>
            {
                new HouseImageReadOnlyDTO(1, 1, "http://example.com/image1.jpg"),
                new HouseImageReadOnlyDTO(2, 1, "http://example.com/image2.jpg")
            };
            imageService.GetImagesByHouseIdAsync(1).Returns(images);

            // Act
            var result = await controller.GetImagesByHouseId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedImages = Assert.IsType<List<HouseImageReadOnlyDTO>>(okResult.Value);
            Assert.Equal(2, returnedImages.Count);
        }

        [Fact]
        public async Task GetImageById_WithValidId_ReturnsOk()
        {
            // Arrange
            var image = new HouseImageReadOnlyDTO(1, 1, "http://example.com/image1.jpg");
            imageService.GetImageByIdAsync(1).Returns(image);

            // Act
            var result = await controller.GetImageById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedImage = Assert.IsType<HouseImageReadOnlyDTO>(okResult.Value);
            Assert.Equal(1, returnedImage.Id);
        }

        [Fact]
        public async Task GetImageById_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            imageService.GetImageByIdAsync(999).Returns((HouseImageReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.GetImageById(999)
            );
        }

        [Fact]
        public async Task DeleteImage_AsOwnerOfHouse_ReturnsNoContent()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var image = new HouseImageReadOnlyDTO(1, 1, "http://example.com/image1.jpg");
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            imageService.GetImageByIdAsync(1).Returns(image);
            houseService.GetHouseByIdAsync(1).Returns(house);
            imageService.DeleteImageAsync(1).Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeleteImage(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteImage_AsNonOwner_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Renter");

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.DeleteImage(1)
            );
        }

        [Fact]
        public async Task DeleteImage_AsOwnerOfDifferentHouse_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 2);
            var image = new HouseImageReadOnlyDTO(1, 1, "http://example.com/image1.jpg");
            var house = new HouseReadOnlyDTO 
            { 
                Id = 1, 
                OwnerId = 1, 
                Name = "Test House", 
                Address = "Test Address", 
                Region = "Test Region", 
                PricePerNight = 100.00m 
            };
            imageService.GetImageByIdAsync(1).Returns(image);
            houseService.GetHouseByIdAsync(1).Returns(house);

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.DeleteImage(1)
            );
        }

        [Fact]
        public async Task DeleteImage_WithNonExistentImage_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            imageService.GetImageByIdAsync(999).Returns((HouseImageReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.DeleteImage(999)
            );
        }

        [Fact]
        public async Task DeleteImage_WithNonExistentHouse_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("Owner", ownerId: 1);
            var image = new HouseImageReadOnlyDTO(1, 999, "http://example.com/image1.jpg");
            imageService.GetImageByIdAsync(1).Returns(image);
            houseService.GetHouseByIdAsync(999).Returns((HouseReadOnlyDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.DeleteImage(1)
            );
        }
    }
}
