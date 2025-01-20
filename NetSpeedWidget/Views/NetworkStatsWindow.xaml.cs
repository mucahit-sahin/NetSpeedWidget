using System.Windows;
using System.Windows.Input;
using NetSpeedWidget.ViewModels;

namespace NetSpeedWidget.Views
{
    public partial class NetworkStatsWindow : Window
    {
        private readonly NetworkStatsViewModel _viewModel;

        public NetworkStatsWindow()
        {
            InitializeComponent();
            _viewModel = new NetworkStatsViewModel();
            DataContext = _viewModel;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}