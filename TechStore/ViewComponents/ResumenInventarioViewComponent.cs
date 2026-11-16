using Microsoft.AspNetCore.Mvc;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.ViewComponents
{
    
    // (consulta la base de datos con el servicio) y se puede usar en cualquier vista con
    // @await Component.InvokeAsync("ResumenInventario")
    public class ResumenInventarioViewComponent : ViewComponent
    {
        private readonly IProductoService _productos;

        public ResumenInventarioViewComponent(IProductoService productos)
        {
            _productos = productos;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var productos = await _productos.ObtenerTodos();
            var activos = productos.Where(p => p.Estado).ToList();

            var modelo = new ResumenInventarioViewModel
            {
                TotalProductos = productos.Count,
                ProductosActivos = activos.Count,
                StockBajo = activos.Count(p => p.Stock <= 5),
                ValorInventario = activos.Sum(p => p.Precio * p.Stock)
            };

            // Busca la vista en Views/Shared/Components/ResumenInventario/Default.cshtml
            return View(modelo);
        }
    }
}
