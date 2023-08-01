using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SkiaSharp;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.Controllers;
using SondeoBackend.DTO.Registros;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;

namespace SondeoBackend.Test
{
    [TestFixture]
    public class SincronizacionControllerTests
    {
        private SincronizacionController _controller;
        private DataContext _dataContext;
        private IHubContext<Hubs> _hubContext;
        private UserManager<CustomUser> _userManager;
        private List<Medicion> _mediciones;
        private List<Local> _locales;
        private List<Encuesta> _encuestas;
        private List<Producto> _productos;
        private List<DetalleEncuesta> _detalleEncuestas;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _dataContext = new DataContext(options);
            _dataContext.Canales.Add(new Canal { Id=1, NombreCanal = "Canal1" });
            _dataContext.Ciudades.Add(new Ciudad { Id = 1, NombreCiudad = "Ciudad1" });
            _dataContext.SaveChanges();
            // Set up the mock DataContext
            _mediciones = new List<Medicion>
            {
                new Medicion { Id = 1, nombreMedicion = "Medicion 1", Activa = true, CiudadId = 1 },
                new Medicion { Id = 2, nombreMedicion = "Medicion 2", Activa = false, CiudadId = 1 },
                new Medicion { Id = 3, nombreMedicion = "Medicion 3", Activa = true, CiudadId = 1 },
            };

            foreach(Medicion medicion in _mediciones)
            {
                _dataContext.Mediciones.Add(medicion);
                _dataContext.SaveChanges();
            }

            _locales = new List<Local>
            {
                new Local { Id = 1, Nombre = "Local 1", Direccion = "Address 1", Latitud = 1.0f, Longitud = 2.0f, Habilitado = true, CanalId = 1 },
                new Local { Id = 2, Nombre = "Local 2", Direccion = "Address 2", Latitud = 3.0f, Longitud = 4.0f, Habilitado = false, CanalId = 2 },
            };

            foreach (Local local in _locales)
            {
                _dataContext.Locales.Add(local);
                _dataContext.SaveChanges();
            }

            _encuestas = new List<Encuesta>
            {
                new Encuesta { Id = 1, FechaInicio = DateTime.Now.AddDays(-3), FechaCierre = null, DiasTrabajados = 0, CustomUserId = 12345, LocalId = 1, MedicionId = 1 },
                new Encuesta { Id = 2, FechaInicio = DateTime.Now.AddDays(-2), FechaCierre = null, DiasTrabajados = 0, CustomUserId = 123456, LocalId = 2, MedicionId = 1 },
            };

            foreach (Encuesta encuesta in _encuestas)
            {
                _dataContext.Encuestas.Add(encuesta);
                _dataContext.SaveChanges();
            }

            _productos = new List<Producto>
            {
                new Producto { BarCode = 12345, Nombre = "Product 1", CategoriaId = 1, MarcaId = 1, PropiedadesId = 1 },
                new Producto { BarCode = 123456, Nombre = "Product 2", CategoriaId = 2, MarcaId = 2, PropiedadesId = 2 },
            };

            foreach (Producto producto in _productos)
            {
                _dataContext.Productos.Add(producto);
                _dataContext.SaveChanges();
            }

            _detalleEncuestas = new List<DetalleEncuesta>
            {
                new DetalleEncuesta { Id = 1, StockInicial = 100, StockFinal = -1, Compra = 10, Pvd = 50, Pvp = 80, EncuestaId = 1, ProductoId = 12345 },
                new DetalleEncuesta { Id = 2, StockInicial = 200, StockFinal = -1, Compra = 20, Pvd = 100, Pvp = 150, EncuestaId = 2, ProductoId = 12345 },
            };

            foreach (DetalleEncuesta detalleEncuesta in _detalleEncuestas)
            {
                _dataContext.DetalleEncuestas.Add(detalleEncuesta);
                _dataContext.SaveChanges();
            }


            // Set up the mock IHubContext
            var mockHubContext = new Mock<IHubContext<Hubs>>();
            _hubContext = mockHubContext.Object;

            // Set up the mock UserManager
            var mockUserManager = new Mock<UserManager<CustomUser>>(Mock.Of<IUserStore<CustomUser>>(), null, null, null, null, null, null, null, null);
            _userManager = mockUserManager.Object;

            _controller = new SincronizacionController(_dataContext, null, null, _hubContext, _userManager);
        }

        [Test]
        public async Task CrearEncuesta_Should_Create_New_Encuesta()
        {
            var encuesta = new Encuesta
            {
                FechaInicio = DateTime.Now.AddDays(-1),
                FechaCierre = null,
                DiasTrabajados = 0,
                CustomUserId = 12345,
                LocalId = 1,
                MedicionId = 1,
            };            
            await _dataContext.Encuestas.AddAsync(encuesta);
            await _dataContext.SaveChangesAsync();

            var confirm = await _dataContext.Encuestas.FindAsync(encuesta.Id);
            Assert.IsTrue(confirm != null);
        }


        [TearDown]
        public void TearDown()
        {
            // Clean up resources if needed
            _controller = null;
            _dataContext = null;
            _hubContext = null;
            _userManager = null;
            _mediciones = null;
            _locales = null;
            _encuestas = null;
            _productos = null;
            _detalleEncuestas = null;
        }
    }
}
