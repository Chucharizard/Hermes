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

namespace Hermes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Actualizar el ícono del botón maximizar según el estado de la ventana
            this.StateChanged += MainWindow_StateChanged;
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
            else
            {
                // Click simple: Arrastrar ventana
                if (this.WindowState == WindowState.Maximized)
                {
                    // Si está maximizada, restaurar primero
                    this.WindowState = WindowState.Normal;

                    // Ajustar la posición de la ventana al cursor
                    var point = PointToScreen(e.GetPosition(this));
                    this.Left = point.X - (this.RestoreBounds.Width / 2);
                    this.Top = point.Y - 20;
                }
                this.DragMove();
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