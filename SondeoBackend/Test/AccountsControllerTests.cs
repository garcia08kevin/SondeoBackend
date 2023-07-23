using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.Controllers.UserManagement.Administrador;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.UserControl;

namespace SondeoBackend.Test
{
    [TestClass]
    public class AccountsControllerTests
    {
        private AccountsController _controller;
        private DataContext _context;
        private Mock<UserManager<CustomUser>> _userManagerMock;
        private Mock<RoleManager<CustomRole>> _roleManagerMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _userManagerMock = new Mock<UserManager<CustomUser>>(
                Mock.Of<IUserStore<CustomUser>>(),
                null, null, null, null, null, null, null, null);

            _roleManagerMock = new Mock<RoleManager<CustomRole>>(
                Mock.Of<IRoleStore<CustomRole>>(),
                null, null, null, null);

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new DataContext(options);

            _controller = new AccountsController(
                _context,
                Mock.Of<SignInManager<CustomUser>>(),
                _userManagerMock.Object,
                Mock.Of<IConfiguration>(),
                _roleManagerMock.Object,
                Mock.Of<ILogger<AccountsController>>());
        }

        [TestMethod]
        public async Task Register_Should_Return_BadRequest_When_ModelState_Is_Invalid()
        {
            _controller.ModelState.AddModelError("error", "Model error");
            var userRegistration = new UserRegistration();

            var result = await _controller.Register(userRegistration);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Register_Should_Return_BadRequest_When_User_Exists()
        {
            var userRegistration = new UserRegistration
            {
                UserName = "existing_user",
                Role = "Administrador"
            };
            _userManagerMock.Setup(m => m.FindByNameAsync(userRegistration.UserName))
                .ReturnsAsync(new CustomUser());

            var result = await _controller.Register(userRegistration);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            var userResult = (UserResult)badRequestResult.Value;
            Assert.IsFalse(userResult.Result);
            Assert.AreEqual("El nombre de usuario ingresado ya existe", userResult.Respose);
        }

        [TestMethod]
        public async Task Register_Should_Return_BadRequest_When_Role_Does_Not_Exist()
        {
            var userRegistration = new UserRegistration
            {
                UserName = "new_user",
                Role = "NonExistentRole"
            };
            _userManagerMock.Setup(m => m.FindByNameAsync(userRegistration.UserName))
                .ReturnsAsync((CustomUser)null);
            _roleManagerMock.Setup(m => m.FindByNameAsync(userRegistration.Role))
                .ReturnsAsync((CustomRole)null);

            var result = await _controller.Register(userRegistration);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            var userResult = (UserResult)badRequestResult.Value;
            Assert.IsFalse(userResult.Result);
            Assert.AreEqual("El Rol no esta registrado en el sistema", userResult.Respose);
        }


        [TestCleanup]
        public void TestCleanup()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
