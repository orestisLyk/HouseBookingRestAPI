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
    public class UserControllerTests
    {
        private readonly UserController controller;
        private readonly IUserService userService;

        public UserControllerTests()
        {
            userService = Substitute.For<IUserService>();
            controller = new UserController(userService);
        }

        private void SetupUserClaims(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetUserByUsername_AsAdmin_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("admin", "Admin");
            var expectedUser = new UserReadOnlyDTO(1, "testuser", "test@example.com", "Test", "User", "User");
            userService.GetUserByUsernameAsync("testuser").Returns(expectedUser);

            // Act
            var result = await controller.GetUserByUsername("testuser");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserReadOnlyDTO>(okResult.Value);
            Assert.Equal(expectedUser.Username, returnedUser.Username);
        }

        [Fact]
        public async Task GetUserByUsername_AsOwnProfile_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("testuser", "User");
            var expectedUser = new UserReadOnlyDTO(1, "testuser", "test@example.com", "Test", "User", "User");
            userService.GetUserByUsernameAsync("testuser").Returns(expectedUser);

            // Act
            var result = await controller.GetUserByUsername("testuser");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserReadOnlyDTO>(okResult.Value);
            Assert.Equal(expectedUser.Username, returnedUser.Username);
        }

        [Fact]
        public async Task GetUserByUsername_AsNonAdminForDifferentUser_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("otheruser", "User");

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.GetUserByUsername("testuser")
            );
        }

        [Fact]
        public async Task GetUsers_AsAdmin_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("admin", "Admin");
            var users = new List<UserReadOnlyDTO>
            {
                new UserReadOnlyDTO(1, "user1", "user1@example.com", "First", "User", "User"),
                new UserReadOnlyDTO(2, "user2", "user2@example.com", "Second", "User", "User")
            };
            var paginatedResult = new PaginatedResult<UserReadOnlyDTO>(users, 2, 1, 10);
            userService.GetPaginatedUsersAsync(1, 10).Returns(paginatedResult);

            // Act
            var result = await controller.GetUsers(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PaginatedResult<UserReadOnlyDTO>>(okResult.Value);
            Assert.Equal(2, returnedResult.TotalCount);
        }

        [Fact]
        public async Task GetUsers_AsNonAdmin_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("user", "User");

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.GetUsers(1, 10)
            );
        }

        [Fact]
        public async Task DeleteUser_AsAdmin_ReturnsNoContent()
        {
            // Arrange
            SetupUserClaims("admin", "Admin");
            userService.DeleteUserAsync(Arg.Any<UserDeleteDTO>()).Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeleteUser(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_AsNonAdmin_ThrowsForbidden()
        {
            // Arrange
            SetupUserClaims("user", "User");

            // Act & Assert
            await Assert.ThrowsAsync<EntityForbiddenException>(
                async () => await controller.DeleteUser(1)
            );
        }

        [Fact]
        public async Task DeleteUser_WithNonExistentUser_ThrowsNotFoundException()
        {
            // Arrange
            SetupUserClaims("admin", "Admin");
            userService.DeleteUserAsync(Arg.Any<UserDeleteDTO>()).Throws(new EntityNotFoundException("User not found"));

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.DeleteUser(999)
            );
        }
    }
}
