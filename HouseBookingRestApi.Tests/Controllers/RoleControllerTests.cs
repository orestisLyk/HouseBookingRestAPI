using HouseBookingRestApi.Controllers;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace HouseBookingRestApi.Tests.Controllers
{
    public class RoleControllerTests
    {
        private readonly RoleController controller;
        private readonly IRoleService roleService;

        public RoleControllerTests()
        {
            roleService = Substitute.For<IRoleService>();
            controller = new RoleController(roleService);
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOk()
        {
            // Arrange
            var roles = new List<RoleReadOnlyDTO>
            {
                new RoleReadOnlyDTO(1, "Admin", "Administrator role"),
                new RoleReadOnlyDTO(2, "Owner", "Property owner role"),
                new RoleReadOnlyDTO(3, "Renter", "Property renter role")
            };
            roleService.GetAllRolesAsync().Returns(roles);

            // Act
            var result = await controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRoles = Assert.IsType<List<RoleReadOnlyDTO>>(okResult.Value);
            Assert.Equal(3, returnedRoles.Count);
        }

        [Fact]
        public async Task GetAllRoles_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var roles = new List<RoleReadOnlyDTO>();
            roleService.GetAllRolesAsync().Returns(roles);

            // Act
            var result = await controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRoles = Assert.IsType<List<RoleReadOnlyDTO>>(okResult.Value);
            Assert.Empty(returnedRoles);
        }
    }
}
