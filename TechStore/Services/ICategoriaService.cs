using TechStore.Models;
namespace TechStore.Services
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> ObtenerTodos();
        Task<Categoria?> ObtenerPorId(int id);
        Task Agregar(Categoria categoria);
        Task<bool> Editar(Categoria categoria);
        Task<bool> Eliminar(int id);
        Task<bool> TieneProductos(int id);
    }
}
