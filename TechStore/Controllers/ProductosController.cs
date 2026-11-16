using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStore.Models;
using TechStore.Services;
namespace TechStore.Controllers
{
    // Todo el controlador exige sesión iniciada
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly IProductoService _productos;
        private readonly ICategoriaService _categorias;

        // Logger para registrar las operaciones del módulo de productos
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IProductoService productos, ICategoriaService categorias, ILogger<ProductosController> logger)
        {
            _productos = productos;
            _categorias = categorias;
            _logger = logger;
        }

        // Los tres roles pueden consultar productos
        [Authorize(Roles = "Administrador,Vendedor,Cliente")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando el listado de productos");

            try
            {
                var productos = await _productos.ObtenerTodos();

                // El Cliente solo puede consultar los productos disponibles (activos y con stock)
                if (User.IsInRole("Cliente"))
                {
                    productos = productos.Where(p => p.Estado && p.Stock > 0).ToList();
                }

                return View(productos);
            }
            catch (Exception ex)
            {
                // Registrar el error en el log
                _logger.LogError(ex, "Se produjo un error al consultar el listado de productos");
                throw;
            }
        }

        // Los tres roles pueden ver el detalle de un producto
        [Authorize(Roles = "Administrador,Vendedor,Cliente")]
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Consultando los detalles del producto con identificador {ProductoId}", id);

            try
            {
                var producto = await _productos.ObtenerPorId(id);

                if (producto == null)
                {
                    // El producto no fue encontrado
                    _logger.LogWarning("No se encontró el producto solicitado con identificador {ProductoId}", id);
                    return NotFound();
                }

                return View(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del producto con identificador {ProductoId}", id);
                throw;
            }
        }

        private async Task CargarCategorias(int? seleccionada = null)
        {
            ViewBag.Categorias = new SelectList(await _categorias.ObtenerTodos(), "ID", "Nombre", seleccionada);
        }

        // Solo Administrador y Vendedor pueden agregar productos
        [Authorize(Roles = "Administrador,Vendedor")]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Iniciando la operación de creación de un producto");
            await CargarCategorias();
            return View(new Producto());
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Vendedor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio,CategoriaID,Stock,Estado")] Producto producto)
        {
            _logger.LogInformation("Iniciando la creación de un producto");

            if (await _categorias.ObtenerPorId(producto.CategoriaID) == null)
                ModelState.AddModelError("CategoriaID", "Seleccione una categoría existente.");

            if (!ModelState.IsValid)
            {
                // Advertencia: el formulario llegó incompleto o con datos incorrectos
                _logger.LogWarning("El producto enviado no cumple con los requerimientos mínimos");
                await CargarCategorias(producto.CategoriaID);
                return View(producto);
            }

            try
            {
                await _productos.Agregar(producto);
                _logger.LogInformation("Producto creado correctamente con identificador {ProductoId}", producto.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al crear el producto");
                throw;
            }
            finally
            {
                // finally se ejecuta siempre, haya error o no
                _logger.LogInformation("Finalizó la operación de creación del producto");
            }

            TempData["Mensaje"] = "Producto registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Solo Administrador y Vendedor pueden editar productos
        [Authorize(Roles = "Administrador,Vendedor")]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Iniciando la operación de edición del producto con identificador {ProductoId}", id);

            var producto = await _productos.ObtenerPorId(id);
            if (producto == null)
            {
                _logger.LogWarning("No se encontró el producto solicitado para editar con identificador {ProductoId}", id);
                return NotFound();
            }

            await CargarCategorias(producto.CategoriaID);
            return View(producto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Vendedor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nombre,Descripcion,Precio,CategoriaID,Stock,Estado")] Producto producto)
        {
            _logger.LogInformation("Iniciando la edición del producto con identificador {ProductoId}", id);

            if (id != producto.ID)
            {
                // Advertencia: el id de la ruta no coincide con el del formulario
                _logger.LogWarning("El identificador {ProductoId} no coincide con el producto enviado ({ProductoEnviadoId})", id, producto.ID);
                return BadRequest();
            }

            if (await _categorias.ObtenerPorId(producto.CategoriaID) == null)
                ModelState.AddModelError("CategoriaID", "Seleccione una categoría existente.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El producto enviado para editar no cumple con los requerimientos mínimos");
                await CargarCategorias(producto.CategoriaID);
                return View(producto);
            }

            try
            {
                if (!await _productos.Editar(producto))
                {
                    _logger.LogWarning("No se encontró el producto solicitado para editar con identificador {ProductoId}", id);
                    return NotFound();
                }
                _logger.LogInformation("Producto con identificador {ProductoId} actualizado correctamente", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al editar el producto con identificador {ProductoId}", id);
                throw;
            }
            finally
            {
                _logger.LogInformation("Finalizó la operación de edición del producto con identificador {ProductoId}", id);
            }

            TempData["Mensaje"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Solo el Administrador puede eliminar productos
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _productos.ObtenerPorId(id);
            if (producto == null)
            {
                _logger.LogWarning("No se encontró el producto solicitado para eliminar con identificador {ProductoId}", id);
                return NotFound();
            }
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmado(int id)
        {
            _logger.LogInformation("Iniciando la eliminación del producto con identificador {ProductoId}", id);

            try
            {
                if (!await _productos.Eliminar(id))
                {
                    _logger.LogWarning("No se encontró el producto solicitado para eliminar con identificador {ProductoId}", id);
                    return NotFound();
                }
                _logger.LogInformation("Producto con identificador {ProductoId} eliminado correctamente", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar el producto con identificador {ProductoId}", id);
                throw;
            }
            finally
            {
                _logger.LogInformation("Finalizó la operación de eliminación del producto con identificador {ProductoId}", id);
            }

            TempData["Mensaje"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
