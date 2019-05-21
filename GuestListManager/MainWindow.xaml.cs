using GuestListManager.ViewModels;
using System.Windows;

namespace GuestListManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            (DataContext as MainWindowViewModel).PopulateGuestList();
        }
    }
}
