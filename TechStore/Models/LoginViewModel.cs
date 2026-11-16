using System.ComponentModel.DataAnnotations;

namespace TechStore.Models
{
    // Datos del formulario de inicio de sesión
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario o correo electrónico es obligatorio")]
        [Display(Name = "Usuario o correo electrónico")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        // Página que el usuario intentó abrir antes de iniciar sesión
        public string? ReturnUrl { get; set; }
    }
}
