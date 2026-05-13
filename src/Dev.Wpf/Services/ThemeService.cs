using Dev.Core.Services;
using Dev.Wpf.Themes;

namespace Dev.Wpf.Services
{
    public sealed class ThemeService : IThemeService
    {
        public void ApplyTheme(string theme)
        {
            if (theme == "System")
                ThemeManager.ApplySystemTheme();
            else
                ThemeManager.ApplyTheme(theme);
        }
    }
}
