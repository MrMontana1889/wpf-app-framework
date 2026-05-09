using Dev.Core.Services;
using Dev.Wpf.Themes;
using System.Windows;

namespace Dev.Wpf.Services
{
    public sealed class ThemeService : IThemeService
    {
        public void ApplyTheme(string theme)
        {
            var appResources = Application.Current.Resources;
            appResources.MergedDictionaries.Clear();

#pragma warning disable WPF0001
            Application.Current.ThemeMode = theme switch
            {
                "Dark" => System.Windows.ThemeMode.Dark,
                "Light" => System.Windows.ThemeMode.Light,
                _ => System.Windows.ThemeMode.System
            };
#pragma warning restore WPF0001

            if (theme == "System")
                ThemeManager.ApplySystemTheme();
            else
                ThemeManager.ApplyTheme(theme);
        }
    }
}
