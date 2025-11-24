using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Hermes.ViewModels;
using Hermes.Services;

namespace Hermes.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Cargar el último tema usado (de la última sesión)
            var lastTheme = ThemeService.Instance.GetLastUsedTheme();
            ThemeService.Instance.ApplyTheme(lastTheme);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && sender is PasswordBox passwordBox)
            {
                ((LoginViewModel)this.DataContext).Password = passwordBox.Password;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Efecto de glow cuando el input obtiene el foco
        /// </summary>
        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            var control = sender as FrameworkElement;
            var parent = control?.Parent as Grid;
            var border = parent?.Parent as Border;

            if (border?.Effect is DropShadowEffect effect)
            {
                // Animar la opacidad del glow
                var storyboard = new Storyboard();
                var animation = new DoubleAnimation
                {
                    To = 0.6,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(animation, border);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Effect.Opacity"));
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }

        /// <summary>
        /// Efecto de glow cuando el input pierde el foco
        /// </summary>
        private void Input_LostFocus(object sender, RoutedEventArgs e)
        {
            var control = sender as FrameworkElement;
            var parent = control?.Parent as Grid;
            var border = parent?.Parent as Border;

            if (border?.Effect is DropShadowEffect effect)
            {
                // Animar la opacidad del glow a 0
                var storyboard = new Storyboard();
                var animation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(animation, border);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Effect.Opacity"));
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }
    }
}
