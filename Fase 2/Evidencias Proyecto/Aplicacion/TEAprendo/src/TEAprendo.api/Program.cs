using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TEAprendo.Infrastructure.Identity;
using TEAprendo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Conexión con PostgreSQL.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configuración de ASP.NET Core Identity.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Habilita los controladores de la API.
builder.Services.AddControllers();

builder.Services.AddHttpClient("TEAprendoIA", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Servicios:IA"] ??
        "http://localhost:5270");
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Habilita la autorización por roles.
builder.Services.AddAuthorization();

// Permite que el frontend consuma la API con cookies.
builder.Services.AddCors(options =>
{
    options.AddPolicy("TEAprendoClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5157")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// OpenAPI para desarrollo.
builder.Services.AddOpenApi();

var app = builder.Build();

// Crea los roles y el administrador inicial.
using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

    // Crea los roles si todavía no existen.
    await IdentitySeeder.SeedRolesAsync(roleManager);

    // Crea el administrador inicial si todavía no existe.
    await IdentitySeeder.SeedAdminAsync(
        userManager,
        builder.Configuration);
}

// OpenAPI solo en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Permite solicitudes desde el frontend.
app.UseCors("TEAprendoClient");

// Identifica al usuario.
app.UseAuthentication();

// Comprueba sus permisos.
app.UseAuthorization();

// Habilita las rutas de los controladores.
app.MapControllers();

app.Run();
