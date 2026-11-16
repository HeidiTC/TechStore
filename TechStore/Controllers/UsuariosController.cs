using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;

namespace TechStore.Controllers
{
    // Administración de usuarios y roles: solo el Administrador
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            ILogger<UsuariosController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: /Usuarios -> lista de usuarios con su rol
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando la lista de usuarios y roles");

            var lista = new List<UsuarioRolViewModel>();

            // UserManager: se leen los usuarios registrados
            var usuarios = _userManager.Users.ToList();
            foreach (var usuario in usuarios)
            {
                // GetRolesAsync devuelve los roles del usuario
                var roles = await _userManager.GetRolesAsync(usuario);
                lista.Add(new UsuarioRolViewModel
                {
                    Id = usuario.Id,
                    UserName = usuario.UserName ?? "",
                    Email = usuario.Email ?? "",
                    Rol = roles.Count > 0 ? roles[0] : "Sin rol"
                });
            }

            // RoleManager: se leen los roles para llenar la lista desplegable
            ViewBag.Roles = _roleManager.Roles.ToList();

            return View(lista);
        }

        // POST: /Usuarios/CambiarRol -> quita el rol actual y asigna el nuevo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string id, string rol)
        {
            _logger.LogInformation("Iniciando el cambio de rol del usuario {UsuarioId} a {Rol}", id, rol);

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                _logger.LogWarning("No se encontró el usuario {UsuarioId}", id);
                return NotFound();
            }

            // Se verifica que el rol exista antes de asignarlo
            if (!await _roleManager.RoleExistsAsync(rol))
            {
                _logger.LogWarning("Se intentó asignar el rol {Rol}, que no existe", rol);
                TempData["Error"] = "El rol seleccionado no existe.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Cada usuario tiene un solo rol: se quitan los roles actuales y se asigna el nuevo
                var rolesActuales = await _userManager.GetRolesAsync(usuario);
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
                await _userManager.AddToRoleAsync(usuario, rol);

                _logger.LogInformation("Se asignó el rol {Rol} al usuario {Usuario}", rol, usuario.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al cambiar el rol del usuario {UsuarioId}", id);
                throw;
            }

            TempData["Mensaje"] = $"El usuario {usuario.UserName} ahora tiene el rol {rol}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
