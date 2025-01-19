using System.Windows;
using System.Windows.Input;
using NetSpeedWidget.Models;
using NetSpeedWidget.ViewModels;
using NetSpeedWidget.Services;

namespace NetSpeedWidget.Views
{
    public partial class AppDetailsWindow : Window
    {
        private readonly NetworkUsageInfo _selectedApp;
        private readonly AppDetailsViewModel _viewModel;

        public AppDetailsWindow(NetworkUsageInfo selectedApp, NetworkMonitorService networkMonitorService)
        {
            InitializeComponent();
            _selectedApp = selectedApp;
            _viewModel = new AppDetailsViewModel(selectedApp, networkMonitorService);
            DataContext = _viewModel;

            // Position the window near the cursor
            var mousePosition = System.Windows.Forms.Control.MousePosition;
            Left = mousePosition.X + 10;
            Top = mousePosition.Y + 10;

            // Clean up when window is closed
            Closed += (s, e) => _viewModel.Dispose();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Close();
            }
            else
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}