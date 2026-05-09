using System.Windows;

namespace Dev.Wpf.Themes
{
    internal static class ThemeManager
    {
        public static void ApplySystemTheme()
        {
            var theme = WindowsThemeDetector.GetWindowsAppTheme();
            ApplyTheme(theme);
        }

        /// <summary>
        /// Apply a named theme ("System", "Light" or "Dark").
        /// </summary>
        public static void ApplyTheme(string themeName)
        {
            var themeUri = new Uri($"pack://application:,,,/Dev.Wpf;component/Themes/{themeName}Theme.xaml");
            var newThemeDictionary = new ResourceDictionary { Source = themeUri };

            var appResources = System.Windows.Application.Current.Resources;
            ResourceDictionary? existingTheme = null;

            foreach (var dict in appResources.MergedDictionaries)
            {
                if (dict.Source != null && dict.Source.OriginalString.Contains("Theme.xaml"))
                {
                    existingTheme = dict;
                    break;
                }
            }

            if (existingTheme != null)
            {
                appResources.MergedDictionaries.Remove(existingTheme);
            }

            appResources.MergedDictionaries.Add(newThemeDictionary);

            var buttonStyleUri = new Uri($"pack://application:,,,/Dev.Wpf;component/Themes/WinFormsButtonStyle.{themeName}.xaml");
            var buttonStyleDictionary = new ResourceDictionary { Source = buttonStyleUri };

            // Remove any existing WinFormsButtonStyle dictionary
            ResourceDictionary? existingButtonStyle = null;
            foreach (var dict in appResources.MergedDictionaries)
            {
                if (dict.Source != null && dict.Source.OriginalString.Contains("WinFormsButtonStyle"))
                {
                    existingButtonStyle = dict;
                    break;
                }
            }

            if (existingButtonStyle != null)
            {
                appResources.MergedDictionaries.Remove(existingButtonStyle);
            }

            appResources.MergedDictionaries.Add(buttonStyleDictionary);
        }

        public static void ApplyTheme(ThemeMode theme)
        {
#pragma warning disable WPF0001
            switch (theme)
            {
                case ThemeMode.Dark:
                    ApplyTheme("Dark");
                    break;
                case ThemeMode.Light:
                    ApplyTheme("Light");
                    break;
                case ThemeMode.System:
                    var systemTheme = WindowsThemeDetector.GetWindowsAppTheme();
                    if (systemTheme != ThemeMode.System)
                        ApplyTheme(systemTheme);
                    else
                        ApplyTheme(ThemeMode.Light);
                    break;
            }
#pragma warning restore WPF0001
        }
    }
}
