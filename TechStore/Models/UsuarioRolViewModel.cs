namespace TechStore.Models
{
    // Una fila de la pantalla de administración de usuarios
    public class UsuarioRolViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
