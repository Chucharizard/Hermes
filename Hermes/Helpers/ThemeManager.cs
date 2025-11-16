using System;
using System.Windows;

namespace Hermes.Helpers
{
    public static class ThemeManager
    {
        private const string LightThemeUri = "Themes/LightTheme.xaml";
        private const string DarkThemeUri = "Themes/DarkTheme.xaml";

        public enum Theme
        {
            Light,
            Dark
        }

        private static Theme _currentTheme = Theme.Light;

        public static Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ApplyTheme(value);
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static event EventHandler? ThemeChanged;

        public static void Initialize()
        {
            // Cargar tema guardado o usar Light por defecto
            var savedTheme = Properties.Settings.Default.Theme;
            if (Enum.TryParse<Theme>(savedTheme, out var theme))
            {
                CurrentTheme = theme;
            }
            else
            {
                ApplyTheme(Theme.Light);
            }
        }

        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
            SaveTheme();
        }

        private static void ApplyTheme(Theme theme)
        {
            var themeUri = theme == Theme.Light ? LightThemeUri : DarkThemeUri;

            // Buscar y remover el tema anterior
            ResourceDictionary? oldTheme = null;
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict.Source != null &&
                    (dict.Source.OriginalString.Contains("LightTheme") ||
                     dict.Source.OriginalString.Contains("DarkTheme")))
                {
                    oldTheme = dict;
                    break;
                }
            }

            if (oldTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldTheme);
            }

            // Agregar el nuevo tema
            var newTheme = new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }

        private static void SaveTheme()
        {
            Properties.Settings.Default.Theme = CurrentTheme.ToString();
            Properties.Settings.Default.Save();
        }
    }
}
