using HouseBookingRestApi.Controllers;
using HouseBookingRestApi.DTO;
using HouseBookingRestApi.Exceptions;
using HouseBookingRestApi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HouseBookingRestApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AuthController controller;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;

        public AuthControllerTests()
        {
            userService = Substitute.For<IUserService>();
            configuration = Substitute.For<IConfiguration>();
            controller = new AuthController(userService, configuration);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new UserLoginDTO("testuser", "password123");
            var expectedToken = new JwtTokenDTO("test.jwt.token");
            userService.Login(dto).Returns(expectedToken);

            // Act
            var result = await controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedToken = Assert.IsType<JwtTokenDTO>(okResult.Value);
            Assert.Equal(expectedToken.token, returnedToken.token);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var dto = new UserLoginDTO("wronguser", "wrongpassword");
            userService.Login(dto).Throws(new InvalidCredentialsException("Invalid credentials"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await controller.Login(dto)
            );
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            // Arrange
            var dto = new UserRegisterDTO(
                "newuser",
                "password123",
                "newuser@example.com",
                "New",
                "User",
                1
            );
            userService.RegisterUserAsync(dto).Returns(Task.CompletedTask);

            // Act
            var result = await controller.Register(dto);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ThrowsUsernameAlreadyExistsException()
        {
            // Arrange
            var dto = new UserRegisterDTO(
                "existinguser",
                "password123",
                "existing@example.com",
                "Existing",
                "User",
                1
            );
            userService.RegisterUserAsync(dto).Throws(new EntityAlreadyExistsException("Username already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                async () => await controller.Register(dto)
            );
        }

        [Fact]
        public async Task Register_WithInvalidRole_ThrowsEntityNotFoundException()
        {
            // Arrange
            var dto = new UserRegisterDTO(
                "newuser",
                "password123",
                "newuser@example.com",
                "New",
                "User",
                999
            );
            userService.RegisterUserAsync(dto).Throws(new EntityNotFoundException("Role not found"));

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await controller.Register(dto)
            );
        }
    }
}
