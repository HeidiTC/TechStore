using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TechStore.Controllers;
using TechStore.Data;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.Tests;

// PRUEBAS UNITARIAS del controlador de productos
// El logger y el TempData se simulan con Moq; la base de datos es en memoria.
public class ProductosControllerTests
{
    [Fact]
    // Comprueba que Index devuelva todos los productos guardados.
    public async Task Index_DevuelveLaListaDeProductos()
    {
        // Crea un contexto en memoria para la prueba.
        await using var context = CreateContext();
        // Crea una categoría de prueba.
        var categoria = new Categoria { Nombre = "Accesorios", Descripcion = "Accesorios" };
        // Añade la categoría al contexto.
        context.Categorias.Add(categoria);
        // Guarda la categoría y genera su identificador.
        await context.SaveChangesAsync();
        // Inserta dos productos de prueba.
        context.Productos.AddRange(
            new Producto { Nombre = "Mouse USB", Descripcion = "Mouse con cable", Precio = 12.50m, CategoriaID = categoria.ID, Stock = 10, Estado = true },
            new Producto { Nombre = "Teclado HP", Descripcion = "Teclado USB", Precio = 18.99m, CategoriaID = categoria.ID, Stock = 5, Estado = true });
        // Persiste los productos en la base de datos en memoria.
        await context.SaveChangesAsync();
        // Crea el controlador usando el contexto y un logger simulado.
        var controller = CreateController(context);

        // Ejecuta la acción Index del controlador.
        var result = await controller.Index();

        // Comprueba que el resultado sea una vista.
        var view = Assert.IsType<ViewResult>(result);
        // Obtiene el modelo de la vista como una colección de productos.
        var productos = Assert.IsAssignableFrom<IEnumerable<Producto>>(view.Model);
        // Comprueba que la vista contenga los dos productos insertados.
        Assert.Equal(2, productos.Count());
    }

    [Fact]
    // Comprueba que el rol Cliente solo vea los productos disponibles (activos y con stock).
    public async Task Index_ConRolCliente_SoloDevuelveProductosDisponibles()
    {
        // Crea un contexto en memoria para la prueba.
        await using var context = CreateContext();
        // Crea una categoría de prueba.
        var categoria = new Categoria { Nombre = "Accesorios", Descripcion = "Accesorios" };
        // Añade la categoría al contexto.
        context.Categorias.Add(categoria);
        // Guarda la categoría y genera su identificador.
        await context.SaveChangesAsync();
        // Inserta un producto disponible, uno sin stock y uno inactivo.
        context.Productos.AddRange(
            new Producto { Nombre = "Mouse USB", Descripcion = "Disponible", Precio = 12.50m, CategoriaID = categoria.ID, Stock = 10, Estado = true },
            new Producto { Nombre = "Audífonos", Descripcion = "Sin stock", Precio = 49.90m, CategoriaID = categoria.ID, Stock = 0, Estado = true },
            new Producto { Nombre = "Monitor LG", Descripcion = "Inactivo", Precio = 329m, CategoriaID = categoria.ID, Stock = 2, Estado = false });
        // Persiste los productos.
        await context.SaveChangesAsync();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Simula que inició sesión un usuario con el rol Cliente.
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Cliente") }, "Prueba"));

        // Ejecuta la acción Index del controlador.
        var result = await controller.Index();

        // Comprueba que el resultado sea una vista.
        var view = Assert.IsType<ViewResult>(result);
        // Obtiene el modelo de la vista.
        var productos = Assert.IsAssignableFrom<IEnumerable<Producto>>(view.Model);
        // Comprueba que solo se muestre el producto disponible.
        Assert.Single(productos);
        // Comprueba que sea el producto esperado.
        Assert.Equal("Mouse USB", productos.First().Nombre);
    }

    [Fact]
    // Comprueba que Details devuelva NotFound cuando el producto no existe.
    public async Task Details_ConProductoInexistente_DevuelveNotFound()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Solicita los detalles de un identificador inexistente.
        var result = await controller.Details(999);

        // Comprueba que el controlador responda con NotFound.
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Comprueba que Create guarde un producto válido y redirija al listado.
    public async Task Create_ConProductoValido_GuardaElProductoYRedirigeAlIndice()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea la categoría que usará el producto.
        var categoria = new Categoria { Nombre = "Laptops", Descripcion = "Laptops" };
        // Añade la categoría al contexto.
        context.Categorias.Add(categoria);
        // Guarda la categoría.
        await context.SaveChangesAsync();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Construye el producto válido que se enviará a la acción.
        var producto = new Producto { Nombre = "Lenovo IdeaPad 3", Descripcion = "Ryzen 5, 8 GB RAM", Precio = 549m, CategoriaID = categoria.ID, Stock = 6, Estado = true };

        // Ejecuta la acción Create con el producto.
        var result = await controller.Create(producto);

        // Comprueba que la acción devuelva una redirección.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que la redirección apunte a Index.
        Assert.Equal(nameof(ProductosController.Index), redirect.ActionName);
        // Comprueba que se haya guardado un único producto.
        Assert.Equal(1, await context.Productos.CountAsync());
    }

    [Fact]
    // Comprueba que Create no guarde datos cuando el modelo es inválido.
    public async Task Create_ConModeloInvalido_NoGuardaYDevuelveLaVista()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Simula un error de validación del nombre.
        controller.ModelState.AddModelError(nameof(Producto.Nombre), "Ingrese el nombre.");
        // Crea el producto que se enviará con el modelo inválido.
        var producto = new Producto { Nombre = "", Descripcion = "Sin nombre", Precio = 10m, CategoriaID = 1, Stock = 1 };

        // Ejecuta la acción Create.
        var result = await controller.Create(producto);

        // Comprueba que se devuelva nuevamente la vista.
        var view = Assert.IsType<ViewResult>(result);
        // Comprueba que la vista conserve el modelo enviado.
        Assert.Same(producto, view.Model);
        // Comprueba que no se haya guardado ningún producto.
        Assert.Empty(context.Productos);
    }

    [Fact]
    // Comprueba que Edit rechace un identificador distinto al del modelo.
    public async Task Edit_ConIdentificadorDiferente_DevuelveBadRequest()
    {
        // Crea un contexto en memoria.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Crea un producto cuyo Id no coincide con el parámetro de la acción.
        var producto = new Producto { ID = 1, Nombre = "Producto", Descripcion = "X", Precio = 10m, CategoriaID = 1, Stock = 1 };

        // Ejecuta Edit con dos identificadores diferentes.
        var result = await controller.Edit(2, producto);

        // Comprueba que la respuesta sea BadRequest.
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    // Comprueba que DeleteConfirmado elimine un producto existente y redirija al listado.
    public async Task Delete_ConProductoExistente_EliminaElProductoYRedirigeAlIndice()
    {
        // Crea una base de datos en memoria.
        await using var context = CreateContext();
        // Crea la categoría del producto.
        var categoria = new Categoria { Nombre = "Monitores", Descripcion = "Monitores" };
        // Añade la categoría al contexto.
        context.Categorias.Add(categoria);
        // Guarda la categoría.
        await context.SaveChangesAsync();
        // Crea el producto que se eliminará.
        var producto = new Producto { Nombre = "Monitor Samsung", Descripcion = "24 pulgadas", Precio = 139m, CategoriaID = categoria.ID, Stock = 10, Estado = true };
        // Añade el producto al contexto.
        context.Productos.Add(producto);
        // Persiste el producto y genera su identificador.
        await context.SaveChangesAsync();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Ejecuta DeleteConfirmado con el identificador del producto.
        var result = await controller.DeleteConfirmado(producto.ID);

        // Comprueba que la acción redirija después de eliminar.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que la redirección apunte a Index.
        Assert.Equal(nameof(ProductosController.Index), redirect.ActionName);
        // Comprueba que ya no queden productos en la base de datos.
        Assert.Empty(context.Productos);
    }

    [Fact]
    // Comprueba que DeleteConfirmado devuelva NotFound si el producto no existe.
    public async Task Delete_ConProductoInexistente_DevuelveNotFound()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Ejecuta DeleteConfirmado con un identificador inexistente.
        var result = await controller.DeleteConfirmado(999);

        // Comprueba que la respuesta sea NotFound.
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Comprueba con Setup, Returns y Verify que el controlador use el TempData simulado al crear un producto.
    public async Task Create_ConProductoValido_GuardaElMensajeEnTempData()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea la categoría que usará el producto.
        var categoria = new Categoria { Nombre = "Accesorios", Descripcion = "Accesorios" };
        // Añade la categoría al contexto.
        context.Categorias.Add(categoria);
        // Guarda la categoría.
        await context.SaveChangesAsync();
        // Crea un mock para no depender de un sistema de logs real.
        var logger = new Mock<ILogger<ProductosController>>();
        // Crea el mock del proveedor de TempData.
        var tempDataProvider = new Mock<ITempDataProvider>();
        // Configura el mock para que devuelva un diccionario vacío cuando se cargue el TempData.
        tempDataProvider
            .Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());
        // Crea el controlador con los servicios reales y el logger simulado.
        var controller = new ProductosController(new ProductoService(context), new CategoriaService(context), logger.Object);
        // Asigna el TempData simulado al controlador.
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider.Object);
        // Construye el producto válido que se enviará a la acción.
        var producto = new Producto { Nombre = "Mouse Logitech", Descripcion = "Mouse USB", Precio = 12.50m, CategoriaID = categoria.ID, Stock = 25, Estado = true };

        // Ejecuta la acción Create con el producto.
        var result = await controller.Create(producto);

        // Comprueba que la acción redirija al índice.
        Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que el mensaje de confirmación sea el esperado.
        Assert.Equal("Producto registrado correctamente.", controller.TempData["Mensaje"]);
        // Verify: comprueba que el controlador haya usado el TempData simulado una sola vez.
        tempDataProvider.Verify(p => p.LoadTempData(It.IsAny<HttpContext>()), Times.Once());
    }

    // Crea un controlador usando el contexto indicado, los servicios y un logger simulado.
    private static ProductosController CreateController(TechStoreContext context)
    {
        // Crea un mock para no depender de un sistema de logs real.
        var logger = new Mock<ILogger<ProductosController>>();
        // Crea una instancia del controlador con los servicios que usan el contexto en memoria.
        var controller = new ProductosController(new ProductoService(context), new CategoriaService(context), logger.Object);
        // Crea un contexto HTTP para que el controlador pueda revisar el usuario (User).
        var httpContext = new DefaultHttpContext();
        // Asigna el contexto HTTP al controlador.
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        // Crea un mock del TempData, porque el controlador guarda ahí los mensajes.
        var tempDataProvider = new Mock<ITempDataProvider>();
        // Asigna el TempData al controlador para que no sea null durante la prueba.
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
        // Devuelve el controlador listo.
        return controller;
    }

    // Crea un contexto conectado a una base de datos EF Core en memoria.
    private static TechStoreContext CreateContext(string? databaseName = null)
    {
        // Configura las opciones del contexto.
        var options = new DbContextOptionsBuilder<TechStoreContext>()
            // Usa el nombre recibido o genera uno nuevo para aislar la prueba.
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            // Finaliza la construcción de opciones.
            .Options;
        // Devuelve el contexto configurado.
        return new TechStoreContext(options);
    }
}
