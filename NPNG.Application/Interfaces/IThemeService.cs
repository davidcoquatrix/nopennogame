using NPNG.Application.Models;

namespace NPNG.Application.Interfaces;

public interface IThemeService
{
    Task<ThemePreference> GetPreferenceAsync();
    Task SetPreferenceAsync(ThemePreference preference);
}
