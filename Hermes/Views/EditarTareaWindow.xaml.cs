using System.Windows;
using Hermes.Models;
using Hermes.ViewModels;

namespace Hermes.Views
{
    public partial class EditarTareaWindow : Window
    {
        public EditarTareaWindow(Tarea tarea)
        {
            InitializeComponent();
            DataContext = new EditarTareaViewModel(tarea);
        }
    }
}
