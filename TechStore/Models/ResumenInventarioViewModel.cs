namespace TechStore.Models
{
    // Datos que muestra el View Component ResumenInventario
    public class ResumenInventarioViewModel
    {
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
        public int StockBajo { get; set; }          // productos activos con 5 unidades o menos
        public decimal ValorInventario { get; set; } // suma de Precio x Stock
    }
}
