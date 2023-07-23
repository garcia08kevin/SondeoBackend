using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.Controllers.UserManagement.Users;
using SondeoBackend.Models;
using Microsoft.AspNetCore.Mvc;
using SondeoBackend.DTO.Result;

namespace SondeoBackend.Test
{
    [TestClass]
    public class UsersControllerTests
    {
        private UsersController _controller;
        private Mock<UserManager<CustomUser>> _userManagerMock;
        private Mock<IHubContext<Hubs>> _hubsMock;
        private DataContext _context;

        [TestInitialize]
        public void TestInitialize()
        {
            // Create mock instances of the dependencies
            _userManagerMock = new Mock<UserManager<CustomUser>>(
                Mock.Of<IUserStore<CustomUser>>(),
                null, null, null, null, null, null, null, null);

            _hubsMock = new Mock<IHubContext<Hubs>>();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new DataContext(options);

            // Create the controller with mock dependencies
            _controller = new UsersController(
                _userManagerMock.Object,
                _hubsMock.Object,
                Mock.Of<ILogger<UsersController>>(),
                _context);
        }

        [TestMethod]
        public async Task ChangeImagen_Should_Return_Ok_When_User_Exists_And_Imagen_Provided()
        {
            // Arrange
            int userId = 1;
            var userExist = new CustomUser { Id = userId };
            var imagen = new Mock<IFormFile>();
            byte[] imageBytes = new byte[10]; // Sample image bytes
            imagen.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(imageBytes));
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(userExist);

            // Act
            var result = await _controller.ChangeImagen(imagen.Object, userId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var userResult = (UserResult)okResult.Value;
            Assert.IsTrue(userResult.Result);
            Assert.AreEqual("Imagen cambiada exitosamente", userResult.Respose);
            Assert.AreEqual(imageBytes, userExist.Imagen);
            _context.Entry(userExist).State = Microsoft.EntityFrameworkCore.EntityState.Detached; // Detach the entity from the context
        }

        [TestMethod]
        public async Task ChangeImagen_Should_Return_Ok_When_User_Exists_And_Imagen_Not_Provided()
        {
            // Arrange
            int userId = 1;
            var userExist = new CustomUser { Id = userId };
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(userExist);

            // Act
            var result = await _controller.ChangeImagen(null, userId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var userResult = (UserResult)okResult.Value;
            Assert.IsFalse(userResult.Result);
            Assert.AreEqual("No se encontro imagen por agregar o remplazar", userResult.Respose);
            Assert.IsNull(userExist.Imagen);
            _context.Entry(userExist).State = Microsoft.EntityFrameworkCore.EntityState.Detached; // Detach the entity from the context
        }

        // Add more unit tests for other methods in the UsersController class as needed

        [TestCleanup]
        public void TestCleanup()
        {
            // Cleanup resources after each test
            _context.Database.EnsureDeleted();
        }
    }
}
