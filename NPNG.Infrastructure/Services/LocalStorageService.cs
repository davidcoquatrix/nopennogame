using System.Text.Json;
using Microsoft.JSInterop;
using NPNG.Application.Interfaces;

namespace NPNG.Infrastructure.Services;

/// <summary>
/// Wrapper JS Interop générique pour le localStorage : centralise la sérialisation JSON
/// et la tolérance aux entrées corrompues, partagées par tous les repositories LocalStorage.
/// </summary>
public class LocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task<IEnumerable<T>> GetAllItemsAsync<T>(string keyPrefix)
    {
        var script = $$"""
            (() => {
                const items = [];
                for (let i = 0; i < localStorage.length; i++) {
                    const key = localStorage.key(i);
                    if (key.startsWith('{{keyPrefix}}')) {
                        items.push(localStorage.getItem(key));
                    }
                }
                return items;
            })()
            """;

        var jsonArray = await jsRuntime.InvokeAsync<string[]>("eval", script);

        var items = new List<T>();
        foreach (var json in jsonArray)
        {
            try
            {
                var item = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                if (item is not null)
                {
                    items.Add(item);
                }
            }
            catch (JsonException)
            {
                // Ignorer les entrées corrompues
            }
        }

        return items;
    }
}
