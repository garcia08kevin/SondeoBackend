using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SondeoBackend.Context;
using SondeoBackend.Controllers;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;

namespace SondeoBackend.Test
{
    [TestClass]
    public class ManageLocalesControllerTests
    {
        private ManageLocalesController _controller;
        private DataContext _context;
        private List<Local> _locales;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new DataContext(options);

            _locales = new List<Local>
            {
                new Local { Id = 1, Nombre = "Local1", Habilitado = true, Canal = new Canal { NombreCanal = "Canal1" } },
                new Local { Id = 2, Nombre = "Local2", Habilitado = true, Canal = new Canal { NombreCanal = "Canal2" } },
            };
            _context.Locales.AddRange(_locales);
            _context.SaveChanges();

            _controller = new ManageLocalesController(_context);
        }

        [TestMethod]
        public async Task GetLocales_Should_Return_All_Locales()
        {

            var result = await _controller.GetLocales();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Value, typeof(List<LocalDto>));
            Assert.AreEqual(_locales.Count, result.Value.Count());
        }

        [TestMethod]
        public async Task GetLocalById_Should_Return_Local_With_Specified_Id()
        {
            int id = 1;

            var result = await _controller.GetLocalById(id);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Value, typeof(LocalDto));
            Assert.AreEqual(id, result.Value.First().Id);
        }

        [TestMethod]
        public async Task GetLocalById_Should_Return_BadRequest_When_Local_Not_Found()
        {
            int id = 999;

            var result = await _controller.GetLocalById(id);

            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }


        [TestCleanup]
        public void TestCleanup()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
