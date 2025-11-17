using System.Windows;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class DetalleTareaWindow : Window
    {
        public DetalleTareaWindow(Tarea tarea)
        {
            InitializeComponent();
            DataContext = new DetalleTareaViewModel(tarea);
        }
    }
}
