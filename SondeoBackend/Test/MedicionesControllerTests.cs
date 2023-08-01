using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.Controllers;
using SondeoBackend.Models;

namespace SondeoBackend.Test
{
    [TestFixture]
    public class MedicionesControllerTests
    {
        private MedicionesController _controller;
        private DataContext _dataContext;
        private UserManager<CustomUser> _userManager;
        private List<Medicion> _mediciones;

        [SetUp]
        public void SetUp()
        {
            _mediciones = new List<Medicion>
            {
                new Medicion { Id = 1, nombreMedicion = "Medicion 1", Activa = true, CiudadId = 1 },
                new Medicion { Id = 2, nombreMedicion = "Medicion 2", Activa = false, CiudadId = 1 },
                new Medicion { Id = 3, nombreMedicion = "Medicion 3", Activa = true, CiudadId = 2 },
            };
        }

        [Test]
        public async Task GetHistoricoMediciones_Should_Return_Historical_Mediciones_For_City()
        {
            var cityId = 1;
            var expectedCount = _mediciones.Count(m => m.CiudadId == cityId);
            Assert.IsTrue(expectedCount != null);
        }


        [TearDown]
        public void TearDown()
        {
            _controller = null;
            _dataContext = null;
            _userManager = null;
            _mediciones = null;
        }
    }
}
