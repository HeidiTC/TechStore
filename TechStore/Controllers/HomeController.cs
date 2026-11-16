using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class HomeController : Controller
    {
        // Unidad 3: logger para dejar registro cuando se muestra la página de error
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Productos()
        {
            return RedirectToAction("Index", "Productos");
        }

        public IActionResult Categorias()
        {
            return RedirectToAction("Index", "Categorias");
        }

        public IActionResult Contacto()
        {
            return View();
        }

        public IActionResult Acerca()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Acción para demostrar el manejo de excepciones:
        // se lanza un error a propósito, se registra con LogError y UseExceptionHandler
        // muestra la página de error personalizada
        [Authorize(Roles = "Administrador")]
        public IActionResult ProbarError()
        {
            _logger.LogInformation("Iniciando la prueba de error");

            try
            {
                throw new InvalidOperationException("Error de prueba para demostrar el manejo de excepciones.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error en la prueba de error");
                throw;
            }
        }

        // Página de error personalizada
        // - UseExceptionHandler la llama cuando hay una excepción no controlada (codigo viene vacío)
        // - UseStatusCodePagesWithReExecute la llama con el código HTTP (ej. 404 cuando no existe el registro)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? codigo)
        {
            var modelo = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                CodigoEstado = codigo
            };

            if (codigo == null)
            {
                _logger.LogError("Se mostró la página de error al usuario. Código de referencia: {RequestId}", modelo.RequestId);
            }
            else
            {
                _logger.LogWarning("Se mostró la página de error con código {CodigoEstado}", codigo);
            }

            return View(modelo);
        }
    }
}
