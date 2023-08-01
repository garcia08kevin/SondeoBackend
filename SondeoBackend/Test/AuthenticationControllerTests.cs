using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SondeoBackend.Configuration;
using SondeoBackend.Controllers.UserManagement.Users;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.UserControl;
using System.Security.Claims;

namespace SondeoBackend.Test
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        private AuthenticationController _controller;
        private Mock<UserManager<CustomUser>> _userManagerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<RoleManager<CustomRole>> _roleManagerMock;
        private Mock<ILogger<AuthenticationController>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<CustomUser>>(
                Mock.Of<IUserStore<CustomUser>>(), null, null, null, null, null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x.GetSection("JwtConfig:Secret").Value).Returns("your_test_secret_key");

            _roleManagerMock = new Mock<RoleManager<CustomRole>>(
                Mock.Of<IRoleStore<CustomRole>>(), null, null, null, null);

            _loggerMock = new Mock<ILogger<AuthenticationController>>();

            _controller = new AuthenticationController(
                _userManagerMock.Object, _configurationMock.Object, _roleManagerMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Login_ValidUser_ReturnsOkWithTokenAndUserData()
        {
            // Arrange
            var userExist = new CustomUser
            {
                Id = 1,
                UserName = "testUserName",
                EmailConfirmed = true,
                CuentaActiva = true,
                Role = "TestRole",
                Name = "John",
                Lastname = "Doe",
                Email = "john.doe@example.com",
                Alias = "john_doe"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync("testUserName"))
                .ReturnsAsync(userExist);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(userExist, "testPassword"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.GetClaimsAsync(userExist))
                .ReturnsAsync(new List<Claim> { new Claim("claim1", "value1") });

            _userManagerMock.Setup(x => x.GetRolesAsync(userExist))
                .ReturnsAsync(new List<string> { "TestRole" });

            // Act
            var loginDto = new UserLogin
            {
                UserName = "testUserName",
                Password = "testPassword"
            };
            var result = await _controller.Login(loginDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var userResult = result.Value as UserResult;
            Assert.IsNotNull(userResult);
            Assert.IsTrue(userResult.Result);
            Assert.IsNotNull(userResult.Token);
            Assert.IsNotNull(userResult.User);
            Assert.AreEqual(1, userResult.User.Id);
            Assert.AreEqual("testUserName", userResult.User.UserName);
            Assert.AreEqual("John", userResult.User.Name);
            Assert.AreEqual("Doe", userResult.User.Lastname);
            Assert.AreEqual("john.doe@example.com", userResult.User.Email);
            Assert.AreEqual(true, userResult.User.Activado);
            Assert.AreEqual(true, userResult.User.CorreoActivado);
            Assert.AreEqual("john_doe", userResult.User.Alias);
            Assert.AreEqual("TestRole", userResult.User.Role);
        }

        [Test]
        public async Task Login_UserNotRegistered_ReturnsOkWithUserResultFalseAndErrorMessage()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByNameAsync("testUserName"))
                .ReturnsAsync((CustomUser)null);

            // Act
            var loginDto = new UserLogin
            {
                UserName = "testUserName",
                Password = "testPassword"
            };
            var result = await _controller.Login(loginDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var userResult = result.Value as UserResult;
            Assert.IsNotNull(userResult);
            Assert.IsFalse(userResult.Result);
            Assert.AreEqual("Usuario No registrado", userResult.Respose);
        }

        [Test]
        public async Task Login_IncorrectPassword_ReturnsOkWithUserResultFalseAndErrorMessage()
        {
            // Arrange
            var userExist = new CustomUser
            {
                Id = 1,
                UserName = "testUserName",
                EmailConfirmed = true,
                CuentaActiva = true,
                Role = "TestRole",
                Name = "John",
                Lastname = "Doe",
                Email = "john.doe@example.com",
                Alias = "john_doe"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync("testUserName"))
                .ReturnsAsync(userExist);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(userExist, "testPassword"))
                .ReturnsAsync(false);

            var loginDto = new UserLogin
            {
                UserName = "testUserName",
                Password = "testPassword"
            };
            var result = await _controller.Login(loginDto) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var userResult = result.Value as UserResult;
            Assert.IsNotNull(userResult);
            Assert.IsFalse(userResult.Result);
            Assert.AreEqual("Clave de Usuario Incorrecta", userResult.Respose);
        }

    }
}
