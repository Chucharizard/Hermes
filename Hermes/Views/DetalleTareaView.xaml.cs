using System.Windows;
using System.Windows.Controls;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class DetalleTareaView : UserControl
    {
        public DetalleTareaView()
        {
            InitializeComponent();
        }

        private void AdjuntosReceptor_DragEnter(object sender, DragEventArgs e)
        {
            // Verificar si el usuario puede subir archivos
            if (DataContext is DetalleTareaViewModel viewModel && !viewModel.PuedeSubirArchivos)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Verificar si son archivos
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DragDropOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void AdjuntosReceptor_DragLeave(object sender, DragEventArgs e)
        {
            // Verificar si realmente salió del control
            var position = e.GetPosition(ScrollViewerAdjuntosReceptor);
            var bounds = new Rect(0, 0, ScrollViewerAdjuntosReceptor.ActualWidth, ScrollViewerAdjuntosReceptor.ActualHeight);

            if (!bounds.Contains(position))
            {
                DragDropOverlay.Visibility = Visibility.Collapsed;
            }

            e.Handled = true;
        }

        private void AdjuntosReceptor_DragOver(object sender, DragEventArgs e)
        {
            // Verificar si el usuario puede subir archivos
            if (DataContext is DetalleTareaViewModel viewModel && !viewModel.PuedeSubirArchivos)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void AdjuntosReceptor_Drop(object sender, DragEventArgs e)
        {
            DragDropOverlay.Visibility = Visibility.Collapsed;

            // Verificar si el usuario puede subir archivos
            if (DataContext is not DetalleTareaViewModel viewModel || !viewModel.PuedeSubirArchivos)
            {
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] archivos = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (archivos != null && archivos.Length > 0)
                {
                    // Llamar al método del ViewModel para procesar los archivos
                    viewModel.ProcesarArchivosArrastrados(archivos);
                }
            }

            e.Handled = true;
        }
    }
}
