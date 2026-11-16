using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models;
namespace TechStore.Services
{
    public class ProductoService : IProductoService
    {
        private readonly TechStoreContext _context;
        public ProductoService(TechStoreContext context)
        {
            _context = context;
        }
        public Task<List<Producto>> ObtenerTodos()
        {
            return _context.Productos.Include(p => p.Categoria).OrderBy(p => p.ID).ToListAsync();
        }
        public async Task<Producto?> ObtenerPorId(int id)
        {
            // Se incluye la categoría para mostrar su nombre en Detalles y Eliminar
            return await _context.Productos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.ID == id);
        }
        public async Task Agregar(Producto producto)
        {
            producto.ID = 0; // Asegurarse de que el ID sea 0 para que se genere automáticamente
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> Editar(Producto producto)
        {
            if (!await _context.Productos.AnyAsync(p => p.ID == producto.ID)) return false;
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Eliminar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return false;
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
