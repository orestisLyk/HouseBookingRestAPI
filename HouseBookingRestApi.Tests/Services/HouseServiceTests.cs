using AutoMapper;
using HouseBookingRestApi.Core;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Models;
using HouseBookingRestApi.Repositories;
using HouseBookingRestApi.Service;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HouseBookingRestApi.Tests.Services
{
    public class HouseServiceTests
    {
        private readonly HouseService service;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<HouseService> logger;

        public HouseServiceTests()
        {
            unitOfWork = Substitute.For<IUnitOfWork>();
            mapper = Substitute.For<IMapper>();
            logger = Substitute.For<ILogger<HouseService>>();
            service = new HouseService(unitOfWork, logger, mapper);
        }

        [Fact]
        public async Task CreateHouseAsync_WithValidData_ReturnsHouseDTO()
        {
            // Arrange
            var dto = new HouseRegisterDTO("Test House", "Description", "Address", "Region", 100.00m);
            var house = new House { Id = 1, Name = "Test House", OwnerId = 1 };
            var houseDTO = new HouseReadOnlyDTO
            {
                Id = 1,
                Name = "Test House",
                Description = "Description",
                Address = "Address",
                Region = "Region",
                PricePerNight = 100.00m,
                OwnerId = 1
            };

            mapper.Map<House>(dto).Returns(house);
            mapper.Map<HouseReadOnlyDTO>(house).Returns(houseDTO);

            // Act
            var result = await service.CreateHouseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test House", result.Name);
            await unitOfWork.HouseRepository.Received(1).AddAsync(Arg.Any<House>());
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task GetHouseByIdAsync_WithValidId_ReturnsHouseDTO()
        {
            // Arrange
            var house = new House { Id = 1, Name = "Test House", IsDeleted = false };
            var houseDTO = new HouseReadOnlyDTO
            {
                Id = 1,
                Name = "Test House",
                Description = "Description",
                Address = "Address",
                Region = "Region",
                PricePerNight = 100.00m,
                OwnerId = 1
            };

            unitOfWork.HouseRepository.GetHouseByIdAsync(1).Returns(house);
            mapper.Map<HouseReadOnlyDTO>(house).Returns(houseDTO);

            // Act
            var result = await service.GetHouseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test House", result.Name);
        }

        [Fact]
        public async Task GetHouseByIdAsync_WithInvalidId_ThrowsEntityNotFoundException()
        {
            // Arrange
            unitOfWork.HouseRepository.GetHouseByIdAsync(999).Returns((House?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.GetHouseByIdAsync(999)
            );
        }

        [Fact]
        public async Task GetHousesByOwnerId_WithValidOwnerId_ReturnsHouseList()
        {
            // Arrange
            var houses = new List<House>
            {
                new House { Id = 1, Name = "House 1", OwnerId = 1 },
                new House { Id = 2, Name = "House 2", OwnerId = 1 }
            };
            var owner = new Owner { Id = 1, Houses = houses };
            var houseDTOs = new List<HouseReadOnlyDTO>
            {
                new HouseReadOnlyDTO { Id = 1, Name = "House 1", Description = "Desc", Address = "Addr", Region = "Region", PricePerNight = 100m, OwnerId = 1 },
                new HouseReadOnlyDTO { Id = 2, Name = "House 2", Description = "Desc", Address = "Addr", Region = "Region", PricePerNight = 150m, OwnerId = 1 }
            };

            unitOfWork.OwnerRepository.GetOwnerByIdAsync(1).Returns(owner);
            mapper.Map<List<HouseReadOnlyDTO>>(Arg.Any<List<House>>()).Returns(houseDTOs);

            // Act
            var result = await service.GetHousesByOwnerId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetHousesByOwnerId_WithInvalidOwnerId_ThrowsEntityNotFoundException()
        {
            // Arrange
            unitOfWork.OwnerRepository.GetOwnerByIdAsync(999).Returns((Owner?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.GetHousesByOwnerId(999)
            );
        }

        [Fact]
        public async Task GetPaginatedHousesAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var houses = new List<House>
            {
                new House { Id = 1, Name = "House 1" },
                new House { Id = 2, Name = "House 2" }
            };
            var paginatedHouses = new PaginatedResult<House>(houses, 10, 1, 5);
            var houseDTOs = new List<HouseReadOnlyDTO>
            {
                new HouseReadOnlyDTO { Id = 1, Name = "House 1", Description = "Desc", Address = "Addr", Region = "Region", PricePerNight = 100m, OwnerId = 1 },
                new HouseReadOnlyDTO { Id = 2, Name = "House 2", Description = "Desc", Address = "Addr", Region = "Region", PricePerNight = 150m, OwnerId = 1 }
            };

            unitOfWork.HouseRepository.GetHousesAsync(1, 5).Returns(paginatedHouses);
            mapper.Map<List<HouseReadOnlyDTO>>(houses).Returns(houseDTOs);

            // Act
            var result = await service.GetPaginatedHousesAsync(1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
        }

        [Fact]
        public async Task UpdateHouseAsync_WithValidData_ReturnsUpdatedHouseDTO()
        {
            // Arrange
            var dto = new HouseUpdateDTO(1, "Updated House", "New Description", "New Address", "New Region", 200.00f);
            var house = new House { Id = 1, Name = "Old House", IsDeleted = false };
            var updatedHouseDTO = new HouseReadOnlyDTO
            {
                Id = 1,
                Name = "Updated House",
                Description = "New Description",
                Address = "New Address",
                Region = "New Region",
                PricePerNight = 200.00m,
                OwnerId = 1
            };

            unitOfWork.HouseRepository.GetHouseByIdAsync(1).Returns(house);
            mapper.Map<HouseReadOnlyDTO>(house).Returns(updatedHouseDTO);

            // Act
            var result = await service.UpdateHouseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated House", result.Name);
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task UpdateHouseAsync_WithInvalidId_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new HouseUpdateDTO(999, "Updated House", "Description", "Address", "Region", 200.00f);
            unitOfWork.HouseRepository.GetHouseByIdAsync(999).Returns((House?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.UpdateHouseAsync(dto)
            );
        }

        [Fact]
        public async Task DeleteHouseAsync_WithValidId_MarksAsDeletedAndReturnsTrue()
        {
            // Arrange
            var house = new House { Id = 1, Name = "Test House", IsDeleted = false };
            unitOfWork.HouseRepository.GetHouseByIdAsync(1).Returns(house);

            // Act
            var result = await service.DeleteHouseAsync(1);

            // Assert
            Assert.True(result);
            Assert.True(house.IsDeleted);
            await unitOfWork.Received(1).SaveAsync();
        }

        [Fact]
        public async Task DeleteHouseAsync_WithInvalidId_ThrowsEntityNotFoundException()
        {
            // Arrange
            unitOfWork.HouseRepository.GetHouseByIdAsync(999).Returns((House?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await service.DeleteHouseAsync(999)
            );
        }
    }
}
