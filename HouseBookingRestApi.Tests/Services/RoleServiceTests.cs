using AutoMapper;
using HouseBookingRestApi.DTO;
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
    public class RoleServiceTests
    {
        private readonly RoleService service;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<RoleService> logger;

        public RoleServiceTests()
        {
            unitOfWork = Substitute.For<IUnitOfWork>();
            mapper = Substitute.For<IMapper>();
            logger = Substitute.For<ILogger<RoleService>>();
            service = new RoleService(unitOfWork, mapper, logger);
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsRoleList()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin", Description = "Administrator" },
                new Role { Id = 2, Name = "Owner", Description = "House Owner" },
                new Role { Id = 3, Name = "Renter", Description = "House Renter" }
            };
            var roleDTOs = new List<RoleReadOnlyDTO>
            {
                new RoleReadOnlyDTO(1, "Admin", "Administrator"),
                new RoleReadOnlyDTO(2, "Owner", "House Owner"),
                new RoleReadOnlyDTO(3, "Renter", "House Renter")
            };

            unitOfWork.RoleRepository.GetAllRolesAsync().Returns(roles);
            mapper.Map<List<RoleReadOnlyDTO>>(roles).Returns(roleDTOs);

            // Act
            var result = await service.GetAllRolesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.Name == "Admin");
            Assert.Contains(result, r => r.Name == "Owner");
            Assert.Contains(result, r => r.Name == "Renter");
        }

        [Fact]
        public async Task GetAllRolesAsync_WithNoRoles_ReturnsEmptyList()
        {
            // Arrange
            var roles = new List<Role>();
            var roleDTOs = new List<RoleReadOnlyDTO>();

            unitOfWork.RoleRepository.GetAllRolesAsync().Returns(roles);
            mapper.Map<List<RoleReadOnlyDTO>>(roles).Returns(roleDTOs);

            // Act
            var result = await service.GetAllRolesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
