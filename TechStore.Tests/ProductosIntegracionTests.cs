using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.Tests;

// PRUEBAS DE INTEGRACIÓN
// Comprueban que dos o más componentes funcionen bien juntos:
public class ProductosIntegracionTests
{
    [Fact]
    // Comprueba el flujo completo: registrar categoría, registrar producto y consultarlo con su categoría.
    public async Task CategoriaYProducto_SeGuardanYSeConsultanJuntos()
    {
        // Crea un contexto en memoria compartido por los dos servicios.
        await using var context = CreateContext();
        // Crea el servicio de categorías.
        var categorias = new CategoriaService(context);
        // Crea el servicio de productos.
        var productos = new ProductoService(context);

        // Registra una categoría con el servicio de categorías.
        await categorias.Agregar(new Categoria { Nombre = "Laptops", Descripcion = "Computadoras portátiles" });
        // Obtiene la categoría guardada.
        var categoria = (await categorias.ObtenerTodos()).Single();
        // Registra un producto de esa categoría con el servicio de productos.
        await productos.Agregar(new Producto { Nombre = "Lenovo IdeaPad 3", Descripcion = "Ryzen 5, 8 GB RAM", Precio = 549m, CategoriaID = categoria.ID, Stock = 6, Estado = true });
        // Consulta los productos.
        var lista = await productos.ObtenerTodos();

        // Comprueba que haya un producto.
        Assert.Single(lista);
        // Comprueba que el producto traiga el nombre de su categoría.
        Assert.Equal("Laptops", lista[0].Categoria!.Nombre);
        // Comprueba que el servicio de categorías detecte que tiene productos.
        Assert.True(await categorias.TieneProductos(categoria.ID));
    }

    [Fact]
    // Comprueba la regla entre los dos servicios: no se elimina una categoría que tiene productos.
    public async Task EliminarCategoria_ConProductos_NoSePermiteHastaEliminarLosProductos()
    {
        // Crea un contexto en memoria compartido por los dos servicios.
        await using var context = CreateContext();
        // Crea el servicio de categorías.
        var categorias = new CategoriaService(context);
        // Crea el servicio de productos.
        var productos = new ProductoService(context);
        // Registra una categoría.
        await categorias.Agregar(new Categoria { Nombre = "Monitores", Descripcion = "Pantallas" });
        // Obtiene la categoría guardada.
        var categoria = (await categorias.ObtenerTodos()).Single();
        // Crea un producto de esa categoría.
        var monitor = new Producto { Nombre = "Samsung 24", Descripcion = "IPS", Precio = 139m, CategoriaID = categoria.ID, Stock = 10, Estado = true };
        // Registra el producto.
        await productos.Agregar(monitor);

        // Intenta eliminar la categoría que tiene un producto.
        var primerIntento = await categorias.Eliminar(categoria.ID);
        // Elimina el producto.
        var productoEliminado = await productos.Eliminar(monitor.ID);
        // Vuelve a intentar eliminar la categoría.
        var segundoIntento = await categorias.Eliminar(categoria.ID);

        // Comprueba que la primera vez no se permitió.
        Assert.False(primerIntento);
        // Comprueba que el producto sí se eliminó.
        Assert.True(productoEliminado);
        // Comprueba que la segunda vez sí se eliminó la categoría.
        Assert.True(segundoIntento);
        // Comprueba que ya no queden categorías.
        Assert.Empty(await context.Categorias.ToListAsync());
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
