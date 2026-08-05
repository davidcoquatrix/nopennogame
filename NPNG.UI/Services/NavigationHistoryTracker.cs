using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace NPNG.UI.Services;

/// <summary>
/// Suit le nombre de navigations internes effectuées dans la SPA depuis son démarrage,
/// pour permettre un vrai "retour arrière" (history.back) sans dépendre de document.referrer
/// (qui ne reflète que le chargement initial du document, jamais le routage client Blazor).
/// </summary>
public class NavigationHistoryTracker : IDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;
    private int _navigationCount;

    public NavigationHistoryTracker(NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _navigationCount++;
    }

    /// <summary>
    /// Revient à l'écran précédent s'il existe un historique de navigation interne à la SPA,
    /// sinon retourne à l'accueil (ex : accès direct via URL ou lancement depuis l'icône PWA).
    /// </summary>
    public async Task GoBackOrHomeAsync()
    {
        if (_navigationCount > 0)
        {
            await _jsRuntime.InvokeVoidAsync("history.back");
        }
        else
        {
            _navigationManager.NavigateTo("");
        }
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
    }
}
