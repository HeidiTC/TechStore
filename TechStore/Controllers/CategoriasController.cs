using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;
using TechStore.Services;
namespace TechStore.Controllers
{
    // La administración de categorías es solo para el Administrador
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ICategoriaService _categorias;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(ICategoriaService categorias, ILogger<CategoriasController> logger)
        {
            _categorias = categorias;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando el listado de categorías");
            return View(await _categorias.ObtenerTodos());
        }
        public IActionResult Create() { return View(new Categoria()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("La categoría enviada no cumple con los requerimientos mínimos");
                return View(categoria);
            }

            try
            {
                await _categorias.Agregar(categoria);
                _logger.LogInformation("Categoría creada correctamente con identificador {CategoriaId}", categoria.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al crear la categoría");
                throw;
            }

            TempData["Mensaje"] = "Categoría registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _categorias.ObtenerPorId(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nombre,Descripcion")] Categoria categoria)
        {
            if (id != categoria.ID) return BadRequest();
            if (!ModelState.IsValid) return View(categoria);

            try
            {
                if (!await _categorias.Editar(categoria)) return NotFound();
                _logger.LogInformation("Categoría con identificador {CategoriaId} actualizada correctamente", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al editar la categoría con identificador {CategoriaId}", id);
                throw;
            }

            TempData["Mensaje"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _categorias.ObtenerPorId(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmado(int id)
        {
            if (await _categorias.ObtenerPorId(id) == null) return NotFound();

            try
            {
                if (!await _categorias.Eliminar(id))
                {
                    _logger.LogWarning("No se puede eliminar la categoría {CategoriaId} porque tiene productos", id);
                    TempData["Error"] = "No se puede eliminar una categoría con productos.";
                    return RedirectToAction(nameof(Index));
                }
                _logger.LogInformation("Categoría con identificador {CategoriaId} eliminada correctamente", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar la categoría con identificador {CategoriaId}", id);
                throw;
            }

            TempData["Mensaje"] = "Categoría eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
