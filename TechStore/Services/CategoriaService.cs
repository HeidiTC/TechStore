using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models;
namespace TechStore.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly TechStoreContext _context;
        public CategoriaService(TechStoreContext context)
        {
            _context = context;
        }
        public Task<List<Categoria>> ObtenerTodos()
        {
            return _context.Categorias.OrderBy(p => p.ID).ToListAsync();
        }
        public async Task<Categoria?> ObtenerPorId(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }
        public async Task Agregar(Categoria categoria)
        {
            categoria.ID = 0; // Asegurarse de que el ID sea 0 para que se genere automáticamente
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> Editar(Categoria categoria)
        {
            if (!await _context.Categorias.AnyAsync(p => p.ID == categoria.ID)) return false;
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Eliminar(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return false;
            if (await TieneProductos(id)) return false;
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> TieneProductos(int id)
        {
            return _context.Productos.AnyAsync(p => p.CategoriaID == id);
        }
    }
}
