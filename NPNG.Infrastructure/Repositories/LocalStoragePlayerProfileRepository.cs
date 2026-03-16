using System.Text.Json;
using Microsoft.JSInterop;
using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;

namespace NPNG.Infrastructure.Repositories;

public class LocalStoragePlayerProfileRepository(IJSRuntime jsRuntime) : IPlayerProfileRepository
{
    private const string ProfileKey = "npng_default_profile";
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<Player?> GetDefaultProfileAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ProfileKey);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Player>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveDefaultProfileAsync(Player profile)
    {
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", ProfileKey, json);
    }
}
