using Dev.Core.Services;
using Dev.Wpf.Themes;
using System;

namespace Dev.Wpf.Services
{
    public sealed class ThemeService : IThemeService
    {
        public string CurrentTheme { get; private set; } = "Light";

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public void ApplyTheme(string theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            var resolvedTheme = ResolveTheme(theme);

            if (theme == "System")
                ThemeManager.ApplySystemTheme();
            else
                ThemeManager.ApplyTheme(theme);

            CurrentTheme = resolvedTheme;
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(resolvedTheme));
        }

        private static string ResolveTheme(string theme)
        {
            if (!string.Equals(theme, "System", StringComparison.Ordinal))
                return theme;

            var systemTheme = WindowsThemeDetector.GetWindowsAppTheme();
            return systemTheme switch
            {
                ThemeMode.Dark => "Dark",
                _ => "Light"
            };
        }
    }
}
