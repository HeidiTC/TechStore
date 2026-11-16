using TechStore.Models;
namespace TechStore.Services
{
    public interface IProductoService
    {
        Task<List<Producto>> ObtenerTodos();
        Task<Producto?> ObtenerPorId(int id);
        Task Agregar(Producto producto);
        Task<bool> Editar(Producto producto);
        Task<bool> Eliminar(int id);
    }
}
