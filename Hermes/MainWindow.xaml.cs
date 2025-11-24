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

                        // FORZAR REFRESH del sidebar para que se actualicen los colores inmediatamente
                        RefreshSidebar();
                    }
                }
            }
        }

        /// <summary>
        /// Botón manual para refrescar el sidebar
        /// </summary>
        private void RefreshSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshSidebar();
        }

        /// <summary>
        /// Fuerza la actualización visual del sidebar completo
        /// </summary>
        private void RefreshSidebar()
        {
            // Forzar re-evaluación de recursos cambiando el DataContext temporalmente
            var originalContext = SidebarBorder.DataContext;
            SidebarBorder.DataContext = null;

            // Usar Dispatcher para restaurar el DataContext después de un frame
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SidebarBorder.DataContext = originalContext;

                // Forzar actualización completa
                SidebarBorder.InvalidateVisual();
                SidebarBorder.UpdateLayout();
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        /// <summary>
        /// Refresca recursivamente todos los elementos visuales en el árbol
        /// </summary>
        private void RefreshVisualTree(DependencyObject parent)
        {
            if (parent == null) return;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // Si el hijo es un FrameworkElement, forzar actualización
                if (child is FrameworkElement element)
                {
                    element.InvalidateVisual();
                    element.UpdateLayout();
                }

                // Recursión para procesar todos los descendientes
                RefreshVisualTree(child);
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