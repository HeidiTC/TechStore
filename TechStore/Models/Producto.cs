using System.ComponentModel.DataAnnotations;
namespace TechStore.Models
{
    public class Producto
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre.")]
        [StringLength(100)]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Ingrese la descripción.")]
        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";

        [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Ingrese un precio entre 0.01 y 999999.99.")]
        public decimal Precio { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
        [Display(Name = "Categoría")]
        public int CategoriaID { get; set; }

         
        public Categoria? Categoria { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }
        public bool Estado { get; set; } = true;
    }
}
