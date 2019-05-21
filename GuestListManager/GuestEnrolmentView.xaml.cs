using GuestListManager.ViewModels;
using System.Windows;

namespace GuestListManager
{
    /// <summary>
    /// Interaction logic for GuestEnrolmentView.xaml
    /// </summary>
    public partial class GuestEnrolmentView : Window
    {
        public GuestEnrolmentView(GuestEnrolmentVewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
