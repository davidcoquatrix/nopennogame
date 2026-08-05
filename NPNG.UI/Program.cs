using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NPNG.Application.Interfaces;
using NPNG.Application.State;
using NPNG.Infrastructure.Repositories;
using NPNG.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Injection des dépendances pour NPNG
builder.Services.AddScoped<ISessionRepository, LocalStorageSessionRepository>();
builder.Services.AddScoped<IPlayerProfileRepository, LocalStoragePlayerProfileRepository>();
builder.Services.AddScoped<IGameCatalogueService, NPNG.Infrastructure.Services.LocalStorageGameCatalogueService>();
builder.Services.AddScoped<IThemeService, NPNG.Infrastructure.Services.LocalStorageThemeService>();
builder.Services.AddScoped<IAppDataResetService, NPNG.Infrastructure.Services.LocalStorageAppDataResetService>();
builder.Services.AddScoped<GameStateService>();
builder.Services.AddScoped<SessionHistoryService>();

await builder.Build().RunAsync();
