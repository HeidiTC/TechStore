using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.Tests;

// PRUEBAS  del servicio de productos (ProductoService)
public class ProductoServiceTests
{
    [Fact]
    
    public async Task ObtenerTodos_DevuelveLaListaDeProductos()
    {
        // Crea un contexto en memoria para la prueba.
        await using var context = CreateContext();
        // Guarda una categoría y dos productos de prueba.
        await AgregarDatosDePrueba(context);
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);

        // Ejecuta el método que se quiere probar.
        var productos = await service.ObtenerTodos();

        // Comprueba que la lista tenga los dos productos.
        Assert.Equal(2, productos.Count);
        // Comprueba que cada producto traiga su categoría (Include).
        Assert.Equal("Accesorios", productos[0].Categoria!.Nombre);
    }

    [Fact]
    // Comprueba que ObtenerTodos funcione cuando no hay productos.
    public async Task ObtenerTodos_SinProductos_DevuelveListaVacia()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);

        // Ejecuta el método que se quiere probar.
        var productos = await service.ObtenerTodos();

        // Comprueba que la lista esté vacía.
        Assert.Empty(productos);
    }

    [Fact]
    // Comprueba que ObtenerPorId devuelva el producto correcto.
    public async Task ObtenerPorId_ConProductoExistente_DevuelveElProducto()
    {
        // Crea un contexto en memoria.
        await using var context = CreateContext();
        // Crea un producto de prueba.
        var producto = await AgregarProducto(context, "Mouse USB", 12.50m);
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);

        // Busca el producto por su identificador.
        var resultado = await service.ObtenerPorId(producto.ID);

        // Comprueba que sí encontró el producto.
        Assert.NotNull(resultado);
        // Comprueba que el nombre sea el esperado.
        Assert.Equal("Mouse USB", resultado!.Nombre);
    }

    [Fact]
    // Comprueba que ObtenerPorId devuelva null si el producto no existe.
    public async Task ObtenerPorId_ConProductoInexistente_DevuelveNull()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);

        // Busca un identificador que no existe.
        var resultado = await service.ObtenerPorId(999);

        // Comprueba que no encontró nada.
        Assert.Null(resultado);
    }

    [Fact]
    // Comprueba que Agregar guarde el producto en la base de datos.
    public async Task Agregar_ConProductoValido_GuardaElProducto()
    {
        // Crea un contexto en memoria.
        await using var context = CreateContext();
        // Crea la categoría del producto.
        var categoria = await AgregarCategoria(context);
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);
        // Construye el producto que se va a registrar.
        var producto = new Producto { Nombre = "Teclado HP", Descripcion = "Teclado USB", Precio = 18.99m, CategoriaID = categoria.ID, Stock = 15, Estado = true };

        // Ejecuta el método Agregar.
        await service.Agregar(producto);

        // Comprueba que se haya guardado un único producto.
        Assert.Equal(1, await context.Productos.CountAsync());
        // Comprueba que el producto recibió un identificador.
        Assert.True(producto.ID > 0);
    }

    [Fact]
    // Comprueba que Editar actualice los datos de un producto existente.
    public async Task Editar_ConProductoExistente_ActualizaElProducto()
    {
        // Genera un nombre único para la base de datos de esta prueba.
        var databaseName = Guid.NewGuid().ToString();
        // Declara las variables donde se guardarán los identificadores.
        int productoId;
        int categoriaId;
        // Crea un contexto separado para preparar los datos iniciales.
        await using (var seedContext = CreateContext(databaseName))
        {
            // Crea el producto original.
            var producto = await AgregarProducto(seedContext, "Mouse USB", 12.50m);
            // Conserva los identificadores generados.
            productoId = producto.ID;
            categoriaId = producto.CategoriaID;
        }
        // Crea otro contexto para simular una nueva petición.
        await using var context = CreateContext(databaseName);
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);
        // Crea la versión modificada del producto.
        var productoEditado = new Producto { ID = productoId, Nombre = "Mouse USB", Descripcion = "Mouse con cable", Precio = 15.00m, CategoriaID = categoriaId, Stock = 8, Estado = true };

        // Ejecuta el método Editar.
        var resultado = await service.Editar(productoEditado);

        // Comprueba que la edición fue exitosa.
        Assert.True(resultado);
        // Obtiene el producto sin seguimiento para comprobar el valor guardado.
        var productoGuardado = await context.Productos.AsNoTracking().SingleAsync();
        // Comprueba que el precio se haya actualizado.
        Assert.Equal(15.00m, productoGuardado.Precio);
        // Comprueba que el stock se haya actualizado.
        Assert.Equal(8, productoGuardado.Stock);
    }

    [Fact]
    // Comprueba que Editar devuelva false si el producto no existe.
    public async Task Editar_ConProductoInexistente_DevuelveFalse()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);
        // Crea un producto con un identificador que no existe.
        var producto = new Producto { ID = 999, Nombre = "No existe", Descripcion = "X", Precio = 1m, CategoriaID = 1, Stock = 1 };

        // Ejecuta el método Editar.
        var resultado = await service.Editar(producto);

        // Comprueba que la edición no se realizó.
        Assert.False(resultado);
    }

    [Fact]
    // Comprueba que Eliminar quite el producto de la base de datos.
    public async Task Eliminar_ConProductoExistente_EliminaElProducto()
    {
        // Crea un contexto en memoria.
        await using var context = CreateContext();
        // Crea el producto que se eliminará.
        var producto = await AgregarProducto(context, "Producto para eliminar", 9.99m);
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);

        // Ejecuta el método Eliminar.
        var resultado = await service.Eliminar(producto.ID);

        // Comprueba que la eliminación fue exitosa.
        Assert.True(resultado);
        // Comprueba que ya no queden productos en la base de datos.
        Assert.Empty(context.Productos);
    }

    [Fact]
    // Comprueba que Eliminar devuelva false si el producto no existe.
    public async Task Eliminar_ConProductoInexistente_DevuelveFalse()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el servicio usando el contexto.
        var service = new ProductoService(context);

        // Ejecuta Eliminar con un identificador inexistente.
        var resultado = await service.Eliminar(999);

        // Comprueba que no se eliminó nada.
        Assert.False(resultado);
    }

    // Guarda una categoría de prueba y la devuelve.
    private static async Task<Categoria> AgregarCategoria(TechStoreContext context)
    {
        // Crea la categoría.
        var categoria = new Categoria { Nombre = "Accesorios", Descripcion = "Accesorios para computadora" };
        // Añade la categoría al contexto.
        context.Categorias.Add(categoria);
        // Guarda la categoría y genera su identificador.
        await context.SaveChangesAsync();
        // Devuelve la categoría guardada.
        return categoria;
    }

    // Guarda un producto de prueba (con su categoría) y lo devuelve.
    private static async Task<Producto> AgregarProducto(TechStoreContext context, string nombre, decimal precio)
    {
        // Crea la categoría del producto.
        var categoria = await AgregarCategoria(context);
        // Crea el producto.
        var producto = new Producto { Nombre = nombre, Descripcion = "Producto de prueba", Precio = precio, CategoriaID = categoria.ID, Stock = 10, Estado = true };
        // Añade el producto al contexto.
        context.Productos.Add(producto);
        // Guarda el producto y genera su identificador.
        await context.SaveChangesAsync();
        // Devuelve el producto guardado.
        return producto;
    }

    // Guarda una categoría y dos productos de prueba.
    private static async Task AgregarDatosDePrueba(TechStoreContext context)
    {
        // Crea la categoría.
        var categoria = await AgregarCategoria(context);
        // Añade dos productos de la misma categoría.
        context.Productos.AddRange(
            new Producto { Nombre = "Mouse USB", Descripcion = "Mouse con cable", Precio = 12.50m, CategoriaID = categoria.ID, Stock = 10, Estado = true },
            new Producto { Nombre = "Teclado HP", Descripcion = "Teclado en español", Precio = 18.99m, CategoriaID = categoria.ID, Stock = 5, Estado = true });
        // Guarda los productos.
        await context.SaveChangesAsync();
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
