using Microsoft.JSInterop;
using NPNG.Application.Interfaces;

namespace NPNG.Infrastructure.Services;

/// <summary>
/// Supprime toutes les données NPNG du localStorage (profil, favoris, jeux personnalisés,
/// sessions, préférence de thème) en ciblant toutes les clés préfixées "npng_".
/// </summary>
public class LocalStorageAppDataResetService(IJSRuntime jsRuntime) : IAppDataResetService
{
    private const string StorageKeyPrefix = "npng_";

    public async Task ResetAllDataAsync()
    {
        var script = $$"""
            (() => {
                const keys = [];
                for (let i = 0; i < localStorage.length; i++) {
                    const key = localStorage.key(i);
                    if (key.startsWith('{{StorageKeyPrefix}}')) {
                        keys.push(key);
                    }
                }
                keys.forEach(k => localStorage.removeItem(k));
            })()
            """;

        await jsRuntime.InvokeVoidAsync("eval", script);
    }
}
