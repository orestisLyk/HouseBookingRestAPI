using AutoMapper;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HouseBookingRestApi.Tests.Services
{
    public class HouseImageServiceTests
    {
        private readonly HouseImageService service;
        private readonly IUnitOfWork unitOfWork;
        private readonly IImageStorageService imageStorageService;
        private readonly IMapper mapper;
        private readonly ILogger<HouseImageService> logger;

        public HouseImageServiceTests()
        {
            unitOfWork = Substitute.For<IUnitOfWork>();
            imageStorageService = Substitute.For<IImageStorageService>();
            mapper = Substitute.For<IMapper>();
            logger = Substitute.For<ILogger<HouseImageService>>();
            service = new HouseImageService(unitOfWork, imageStorageService, logger, mapper);
        }

        [Fact]
        public async Task CreateImageAsync_WithValidData_CreatesImage()
        {
            // Arrange
            var file = Substitute.For<IFormFile>();
            var dto = new HouseImageCreateDTO(1, file);
            var imageUrl = "https://example.com/image.jpg";

            imageStorageService.UploadImageAsync(1, file).Returns(imageUrl);

            // Act
            await service.CreateImageAsync(dto);

            // Assert
            await imageStorageService.Received(1).UploadImageAsync(1, file);
            await unitOfWork.HouseImageRepository.Received(1).AddAsync(Arg.Is<HouseImage>(h => h.HouseId == 1 && h.Url == imageUrl));
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task GetImagesByHouseIdAsync_ReturnsImageList()
        {
            // Arrange
            var houseImages = new List<HouseImage>
            {
                new HouseImage { Id = 1, HouseId = 1, Url = "url1.jpg" },
                new HouseImage { Id = 2, HouseId = 1, Url = "url2.jpg" }
            };
            var imageDTOs = new List<HouseImageReadOnlyDTO>
            {
                new HouseImageReadOnlyDTO(1, 1, "url1.jpg"),
                new HouseImageReadOnlyDTO(2, 1, "url2.jpg")
            };

            unitOfWork.HouseImageRepository.GetHouseImagesByHouseIdAsync(1).Returns(houseImages);
            mapper.Map<List<HouseImageReadOnlyDTO>>(houseImages).Returns(imageDTOs);

            // Act
            var result = await service.GetImagesByHouseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetImageByIdAsync_WithValidId_ReturnsImageDTO()
        {
            // Arrange
            var houseImage = new HouseImage { Id = 1, HouseId = 1, Url = "url.jpg" };
            var imageDTO = new HouseImageReadOnlyDTO(1, 1, "url.jpg");

            unitOfWork.HouseImageRepository.GetHouseImageByIdAsync(1).Returns(houseImage);
            mapper.Map<HouseImageReadOnlyDTO>(houseImage).Returns(imageDTO);

            // Act
            var result = await service.GetImageByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("url.jpg", result.Url);
        }

        [Fact]
        public async Task GetImageByIdAsync_WithInvalidId_ThrowsEntityNotFoundException()
        {
            // Arrange
            unitOfWork.HouseImageRepository.GetHouseImageByIdAsync(999).Returns((HouseImage?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.GetImageByIdAsync(999)
            );
        }

        [Fact]
        public async Task DeleteImageAsync_WithValidId_MarksAsDeleted()
        {
            // Arrange
            var houseImage = new HouseImage { Id = 1, HouseId = 1, Url = "url.jpg", IsDeleted = false };
            unitOfWork.HouseImageRepository.GetHouseImageByIdAsync(1).Returns(houseImage);

            // Act
            await service.DeleteImageAsync(1);

            // Assert
            Assert.True(houseImage.IsDeleted);
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task DeleteImageAsync_WithInvalidId_ThrowsEntityNotFoundException()
        {
            // Arrange
            unitOfWork.HouseImageRepository.GetHouseImageByIdAsync(999).Returns((HouseImage?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.DeleteImageAsync(999)
            );
        }
    }
}
