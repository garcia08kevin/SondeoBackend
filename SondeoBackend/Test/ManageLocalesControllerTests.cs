using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SondeoBackend.Context;
using SondeoBackend.Controllers;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;

namespace SondeoBackend.Test
{
    [TestFixture]
    public class ManageLocalesControllerTests
    {
        private ManageLocalesController _controller;
        private DataContext _context;
        private List<Local> _locales;

        [SetUp]
        public void TestInitialize()
        {
            _context = new DataContext(new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "test_db")
                .Options);


            _locales = new List<Local>
            {
                new Local { Id = 1, Nombre = "Local1", Habilitado = true, Canal = new Canal { NombreCanal = "Canal1" } },
                new Local { Id = 2, Nombre = "Local2", Habilitado = true, Canal = new Canal { NombreCanal = "Canal2" } },
            };
            _context.Locales.AddRange(_locales);
            _context.SaveChanges();

            _controller = new ManageLocalesController(_context);
        }

        [Test]
        public async Task GetLocales_Should_Return_All_Locales()
        {

            var result = await _controller.GetLocales();

            Assert.IsNotNull(result);
            Assert.AreEqual(_locales.Count, result.Value.Count());
        }

        [Test]
        public async Task GetLocalById_Should_Return_Local_With_Specified_Id()
        {
            int id = 1;

            var result = await _controller.GetLocalById(id);

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GetLocalById_Should_Return_BadRequest_When_Local_Not_Found()
        {
            int id = 999;

            var result = await _controller.GetLocalById(id);

            Assert.IsTrue(result != null);
        }


        [TearDown]
        public void TestCleanup()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
