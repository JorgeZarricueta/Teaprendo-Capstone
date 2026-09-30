using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TEAprendo.Client;
using TEAprendo.Client.Http;
using TEAprendo.Client.Services;

var builder =
    WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>(
    "head::after");

// Handler que envía las cookies de autenticación.
builder.Services.AddTransient<CookieHandler>();

// Cliente HTTP conectado al backend de TEAprendo.
builder.Services
    .AddHttpClient(
        "TEAprendoApi",
        client =>
        {
            client.BaseAddress =
                new Uri("http://localhost:5211/");
        })
    .AddHttpMessageHandler<CookieHandler>();

// Permite inyectar HttpClient directamente.
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("TEAprendoApi"));

// Mantiene la sede seleccionada en el frontend.
builder.Services.AddScoped<SedeContextoService>();

builder.Services.AddScoped<SesionService>();

await builder.Build().RunAsync();