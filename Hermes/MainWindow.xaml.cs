using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hermes.Services;

namespace Hermes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isInitialized = false;

        public MainWindow()
        {
            InitializeComponent();

            // Actualizar el ícono del botón maximizar según el estado de la ventana
            this.StateChanged += MainWindow_StateChanged;

            // Cargar el tema actual
            LoadCurrentTheme();
            _isInitialized = true;
        }

        /// <summary>
        /// Carga el tema actual y actualiza el ComboBox
        /// </summary>
        private void LoadCurrentTheme()
        {
            var currentTheme = ThemeService.Instance.GetCurrentTheme();

            // Establecer el índice del ComboBox según el tema actual
            if (currentTheme == "Emerald")
            {
                ThemeComboBox.SelectedIndex = 0;
            }
            else if (currentTheme == "Purple")
            {
                ThemeComboBox.SelectedIndex = 1;
            }
        }

        /// <summary>
        /// Maneja el cambio de selección en el ComboBox de tema
        /// </summary>
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Evitar cambiar el tema durante la inicialización
            if (!_isInitialized)
                return;

            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var themeName = selectedItem.Tag?.ToString();

                if (!string.IsNullOrEmpty(themeName))
                {
                    // Aplicar el tema a la interfaz
                    if (ThemeService.Instance.ApplyTheme(themeName))
                    {
                        // Guardar la preferencia en la base de datos
                        ThemeService.Instance.SaveUserThemePreference(themeName);
                    }
                }
            }
        }

        /// <summary>
        /// Permite arrastrar la ventana desde la barra de título
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Doble click: Maximizar/Restaurar
                MaximizeButton_Click(sender, e);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Click simple: Arrastrar ventana
                try
                {
                    this.DragMove();
                }
                catch
                {
                    // Ignorar excepciones si DragMove falla
                }
            }
        }

        /// <summary>
        /// Minimiza la ventana
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maximiza o restaura la ventana
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Cierra la aplicación
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Actualiza el ícono del botón maximizar cuando cambia el estado de la ventana
        /// </summary>
        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Content = "❐"; // Ícono para restaurar
            }
            else
            {
                MaximizeButton.Content = "▢"; // Ícono para maximizar
            }
        }
    }
}