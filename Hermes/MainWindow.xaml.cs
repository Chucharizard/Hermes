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
        /// Fuerza la actualización visual del sidebar y main content area
        /// </summary>
        private void RefreshSidebar()
        {
            // RECREAR los LinearGradientBrush con los colores actualizados
            // Este es el único método que funciona cuando DynamicResource no actualiza automáticamente

            try
            {
                // Obtener colores actuales del tema
                var backgroundDarkest = (Color)Application.Current.FindResource("BackgroundDarkestColor");
                var backgroundDark = (Color)Application.Current.FindResource("BackgroundDarkColor");

                // ===== TITLE BAR =====
                // Actualizar el background de la barra de título
                TitleBarBorder.Background = new SolidColorBrush(backgroundDarkest);

                // ===== SIDEBAR =====
                // Crear nuevo LinearGradientBrush vertical para sidebar
                var sidebarBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1)
                };
                sidebarBrush.GradientStops.Add(new GradientStop(backgroundDarkest, 0));
                sidebarBrush.GradientStops.Add(new GradientStop(backgroundDarkest, 0.5));
                sidebarBrush.GradientStops.Add(new GradientStop(backgroundDarkest, 1));
                SidebarBorder.Background = sidebarBrush;

                // ===== USER INFO BORDER =====
                // Crear nuevo LinearGradientBrush para el borde de información del usuario
                var userInfoBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1)
                };
                userInfoBrush.GradientStops.Add(new GradientStop(backgroundDark, 0));
                userInfoBrush.GradientStops.Add(new GradientStop(backgroundDarkest, 1));
                UserInfoBorder.Background = userInfoBrush;

                // Actualizar el BorderBrush del UserInfoBorder
                var borderColorBrush = (SolidColorBrush)Application.Current.FindResource("BorderColor");
                UserInfoBorder.BorderBrush = borderColorBrush;

                // ===== MAIN CONTENT AREA (Dashboard) =====
                // Crear nuevo LinearGradientBrush diagonal para main content
                var mainContentBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1)
                };
                mainContentBrush.GradientStops.Add(new GradientStop(backgroundDarkest, 0));
                mainContentBrush.GradientStops.Add(new GradientStop(backgroundDark, 1));
                MainContentGrid.Background = mainContentBrush;

                // Forzar actualización visual de todos los elementos
                TitleBarBorder.InvalidateVisual();
                TitleBarBorder.UpdateLayout();
                SidebarBorder.InvalidateVisual();
                SidebarBorder.UpdateLayout();
                UserInfoBorder.InvalidateVisual();
                UserInfoBorder.UpdateLayout();
                MainContentGrid.InvalidateVisual();
                MainContentGrid.UpdateLayout();

                // Recorrer elementos hijos para forzar actualización
                RefreshVisualTree(SidebarBorder);
                RefreshVisualTree(MainContentGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar tema: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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