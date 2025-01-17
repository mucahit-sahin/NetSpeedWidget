using System.Windows;
using System.Windows.Input;

namespace NetSpeedWidget.Views
{
    public partial class NetworkUsageWindow : Window
    {
        public NetworkUsageWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}