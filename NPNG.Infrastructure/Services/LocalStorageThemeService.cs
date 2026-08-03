using Microsoft.JSInterop;
using NPNG.Application.Interfaces;
using NPNG.Application.Models;

namespace NPNG.Infrastructure.Services;

/// <summary>
/// Pilote la préférence de thème (Système/Clair/Sombre) via le helper JS `window.npngTheme`
/// défini dans index.html, qui applique l'attribut data-theme sur &lt;html&gt; et persiste
/// le choix en localStorage.
/// </summary>
public class LocalStorageThemeService(IJSRuntime jsRuntime) : IThemeService
{
    public async Task<ThemePreference> GetPreferenceAsync()
    {
        var value = await jsRuntime.InvokeAsync<string>("npngTheme.get");
        return value switch
        {
            "dark" => ThemePreference.Dark,
            "light" => ThemePreference.Light,
            _ => ThemePreference.System
        };
    }

    public async Task SetPreferenceAsync(ThemePreference preference)
    {
        var value = preference switch
        {
            ThemePreference.Dark => "dark",
            ThemePreference.Light => "light",
            _ => "system"
        };

        await jsRuntime.InvokeVoidAsync("npngTheme.set", value);
    }
}
