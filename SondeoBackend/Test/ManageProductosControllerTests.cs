using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;
using SondeoBackend.Context;
using SondeoBackend.Controllers;
using SondeoBackend.Models;

namespace SondeoBackend.Test
{
    [TestFixture]
    public class ManageProductosControllerTests
    {
        private DataContext _context;
        private ManageProductosController _controller;

        [SetUp]
        public void Setup()
        {
            // Mock the DataContext
            _context = new DataContext(new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "test_db")
                .Options);

            // Populate test data
            var productos = new List<Producto>
            {
                new Producto { BarCode = 1, Nombre = "Producto 1", CategoriaId = 1, MarcaId = 1, PropiedadesId = 1 },
                new Producto { BarCode = 2, Nombre = "Producto 2", CategoriaId = 1, MarcaId = 1, PropiedadesId = 2 },
                new Producto { BarCode = 3, Nombre = "Producto 3", CategoriaId = 2, MarcaId = 2, PropiedadesId = 1 }
            };

            foreach(Producto producto in productos)
            {
                _context.Productos.Add(producto);
                _context.SaveChanges();
            }            

            _controller = new ManageProductosController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        #region GetProductos
        [Test]
        public async Task GetProductos_ReturnsListOfProductos()
        {
            // Act
            var result = await _context.Productos.ToListAsync();

            // Assert
            Assert.IsTrue(result.Count() == 3);
        }

        [Test]
        public async Task GetProductos_ReturnsNotFound_WhenNoProductosExist()
        {
            // Arrange
            _context.Productos.RemoveRange(_context.Productos);
            _context.SaveChanges();

            // Act
            var result = await _controller.GetProductos();

            // Assert
            Assert.IsTrue(result.Result == null);
        }
        #endregion

        #region GetProducto
        [Test]
        public async Task GetProducto_ReturnsCorrectProducto_WhenValidIdProvided()
        {
            // Arrange
            long validId = 1;

            // Act
            var result = await _context.Productos.FindAsync(validId);

            // Assert
            Assert.IsTrue(result.BarCode != null);
        }

        [Test]
        public async Task GetProducto_ReturnsBadRequest_WhenInvalidIdProvided()
        {
            // Arrange
            long invalidId = 100;

            // Act
            var result = await _controller.GetProducto(invalidId);

            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }
        #endregion

        // Add more unit tests for other action methods similarly.

        // You can write tests for other action methods similarly using different test cases and assertions.

    }
}