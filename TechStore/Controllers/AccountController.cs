using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;

namespace TechStore.Controllers
{
    // Unidad 3: inicio de sesión, registro, cierre de sesión y acceso denegado con ASP.NET Core Identity
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Se puede entrar con el nombre de usuario o con el correo
            var user = await _userManager.FindByNameAsync(model.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    _logger.LogInformation("El usuario {Usuario} inició sesión con el rol {Rol}", user.UserName, string.Join(", ", roles));

                    // Solo se redirige a direcciones locales (evita redirecciones a otros sitios)
                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("La cuenta {Usuario} quedó bloqueada por intentos fallidos", user.UserName);
                    ModelState.AddModelError(string.Empty, "La cuenta está bloqueada temporalmente por demasiados intentos fallidos.");
                    return View(model);
                }
            }

            // Mismo mensaje si el usuario no existe o si la contraseña es incorrecta (no damos pistas)
            _logger.LogWarning("Intento de inicio de sesión fallido para {Usuario}", model.UserNameOrEmail);
            ModelState.AddModelError(string.Empty, "Las credenciales introducidas no son correctas.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register -> los usuarios nuevos quedan con el rol Cliente
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new IdentityUser { UserName = model.UserName, Email = model.Email };

            // CreateAsync guarda el usuario y la contraseña encriptada (hash)
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Cliente");
                _logger.LogInformation("Se registró el usuario {Usuario} con el rol {Rol}", user.UserName, "Cliente");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("El usuario {Usuario} cerró sesión", User.Identity?.Name);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied -> el usuario inició sesión pero no tiene el rol necesario
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            // returnUrl lo envía Identity y trae la ruta que el usuario intentó abrir
            _logger.LogWarning("El usuario {Usuario} intentó acceder sin permisos a {Ruta}", User.Identity?.Name, returnUrl);
            ViewBag.RutaSolicitada = returnUrl;
            return View();
        }
    }
}
