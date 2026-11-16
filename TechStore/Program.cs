using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using TechStore.Data;
using TechStore.Models;
using TechStore.Services;


var builder = WebApplication.CreateBuilder(args);

// licencia de QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllersWithViews();

// Configurar la cadena de conexión a la base de datos
builder.Services.AddDbContext<TechStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TechStoreDB")));

// Configuración de Identity: usuarios y roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    // Bloqueo de la cuenta: 5 intentos fallidos = 15 minutos bloqueado
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<TechStoreContext>()
.AddErrorDescriber<ErroresIdentityEspanol>(); 

// Configuración de la cookie de autenticación: si no hay sesión, redirige a /Account/Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    // Si inició sesión pero no tiene el rol necesario, lo mandamos a Acceso denegado
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

var app = builder.Build();

// Se crean los roles y los usuarios de prueba
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<TechStoreContext>();

    // 1. Se crean los roles "Administrador", "Vendedor" y "Cliente"
    //    Antes de crear cada rol se verifica que no exista, así no hay duplicados
    string[] roles = { "Administrador", "Vendedor", "Cliente" };
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // 2. Usuarios de prueba y el rol que le toca a cada uno
    //    Todos usan la contraseña TechStore123! y el correo usuario@techstore.com
    var usuariosYRoles = new Dictionary<string, string>
    {
        { "admin", "Administrador" },
        { "vendedor", "Vendedor" },
        { "cliente", "Cliente" }
    };

    foreach (var item in usuariosYRoles)
    {
        var usuario = await userManager.FindByNameAsync(item.Key);
        if (usuario == null)
        {
            // CreateAsync guarda el usuario y la contraseña encriptada (hash)
            usuario = new IdentityUser { UserName = item.Key, Email = $"{item.Key}@techstore.com" };
            await userManager.CreateAsync(usuario, "TechStore123!");
        }

        if (!await userManager.IsInRoleAsync(usuario, item.Value))
        {
            await userManager.AddToRoleAsync(usuario, item.Value);
        }
    }

    // 3. Se asigna el rol "Cliente" a todos los demás usuarios que no tengan rol
    var usuarios = userManager.Users.ToList();
    foreach (var usuario in usuarios)
    {
        var rolesDelUsuario = await userManager.GetRolesAsync(usuario);
        if (rolesDelUsuario.Count == 0)
        {
            var result = await userManager.AddToRoleAsync(usuario, "Cliente");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo asignar el rol Cliente al usuario {usuario.UserName}");
            }
        }
    }

    // 4. Datos de ejemplo para probar los reportes (solo si la base no tiene categorías)
    if (!context.Categorias.Any())
    {
        var accesorios = new Categoria { Nombre = "Accesorios", Descripcion = "Mouse, teclados y otros accesorios para computadora." };
        var laptops = new Categoria { Nombre = "Laptops", Descripcion = "Computadoras portátiles para estudio y oficina." };
        var monitores = new Categoria { Nombre = "Monitores", Descripcion = "Pantallas para oficina y diseño." };
        context.Categorias.AddRange(accesorios, laptops, monitores);
        await context.SaveChangesAsync();

        context.Productos.AddRange(
            new Producto { Nombre = "Mouse USB Logitech M90", Descripcion = "Mouse óptico con cable USB, 1000 DPI.", Precio = 12.50m, CategoriaID = accesorios.ID, Stock = 25, Estado = true },
            new Producto { Nombre = "Teclado HP K1500", Descripcion = "Teclado USB en español, resistente a derrames.", Precio = 18.99m, CategoriaID = accesorios.ID, Stock = 15, Estado = true },
            new Producto { Nombre = "Audífonos JBL Tune 510BT", Descripcion = "Audífonos inalámbricos Bluetooth con micrófono.", Precio = 49.90m, CategoriaID = accesorios.ID, Stock = 0, Estado = true },
            new Producto { Nombre = "Laptop Lenovo IdeaPad 3", Descripcion = "Ryzen 5, 8 GB RAM, SSD 512 GB, pantalla 15.6 pulgadas.", Precio = 549.00m, CategoriaID = laptops.ID, Stock = 6, Estado = true },
            new Producto { Nombre = "Laptop Dell Inspiron 14", Descripcion = "Intel Core i5, 16 GB RAM, SSD 512 GB.", Precio = 729.00m, CategoriaID = laptops.ID, Stock = 3, Estado = true },
            new Producto { Nombre = "Monitor Samsung 24 FHD", Descripcion = "Monitor IPS 24 pulgadas, 75 Hz, HDMI.", Precio = 139.00m, CategoriaID = monitores.ID, Stock = 10, Estado = true },
            new Producto { Nombre = "Monitor LG 27 4K", Descripcion = "Monitor 27 pulgadas UHD para diseño (descontinuado).", Precio = 329.00m, CategoriaID = monitores.ID, Stock = 2, Estado = false });
        await context.SaveChangesAsync();
    }
}

// Manejo global de errores:
// UseExceptionHandler atrapa cualquier excepción no controlada y muestra la página de error
// personalizada, sin enseñar detalles técnicos al usuario.
// Se deja activo también en desarrollo para poder demostrar la página de error.
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Para códigos como 404 (registro que no existe) también se muestra la página de error personalizada
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

app.UseHttpsRedirection();
app.UseRouting();

// Se revisa si hay sesión activa y se crea la cookie de autenticación
app.UseAuthentication();
// Se revisa si el usuario tiene permiso para acceder a la página
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.Run();
