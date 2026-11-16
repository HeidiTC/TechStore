using System.ComponentModel.DataAnnotations;
namespace TechStore.Models
{
    public class Categoria
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Ingrese el nombre.")]
        [StringLength(100)]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Ingrese la descripción.")]
        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";
        public List<Producto> Productos { get; set; } = new List<Producto>();
    }
}
