using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using NetSpeedWidget.ViewModels;
using NetSpeedWidget.Services;
using System.Diagnostics;
using MessageBox = System.Windows.MessageBox;

namespace NetSpeedWidget.Views
{
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly MainViewModel _viewModel;
        private NetworkUsageViewModel _networkUsageViewModel;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Debug.WriteLine("Window initialized");

                _viewModel = (MainViewModel)DataContext;
                if (_viewModel == null)
                {
                    throw new InvalidOperationException("ViewModel is null");
                }
                Debug.WriteLine("ViewModel initialized");

                // Initialize system tray icon
                _notifyIcon = new NotifyIcon
                {
                    Icon = SystemIcons.Application,
                    Visible = true,
                    Text = "Network Speed Widget"
                };
                Debug.WriteLine("NotifyIcon created");

                // Create context menu
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
                contextMenu.Items.Add("Exit", null, (s, e) => _viewModel.ExitCommand.Execute(null));
                _notifyIcon.ContextMenuStrip = contextMenu;
                Debug.WriteLine("Context menu created");

                // Handle double click on tray icon
                _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

                // Update tooltip with network speed
                CompositionTarget.Rendering += (s, e) =>
                {
                    try
                    {
                        if (_notifyIcon != null && _viewModel?.NetworkSpeed != null)
                        {
                            _notifyIcon.Text = $"↓ {_viewModel.NetworkSpeed.DownloadSpeedFormatted}\n↑ {_viewModel.NetworkSpeed.UploadSpeedFormatted}";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating tooltip: {ex.Message}");
                    }
                };

                // Ensure window is visible
                Show();
                Activate();
                Debug.WriteLine("Window shown and activated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow constructor: {ex.Message}");
                MessageBox.Show($"Error initializing application: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void ShowMainWindow()
        {
            try
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
                Debug.WriteLine("Window shown from tray");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing window: {ex.Message}");
                MessageBox.Show($"Error showing window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowNetworkUsageWindow();
            }
            else
            {
                DragMove();
            }
        }

        private void ShowNetworkUsageWindow()
        {
            try
            {
                // Create NetworkUsageViewModel with a new NetworkMonitorService
                var monitorService = new NetworkMonitorService();
                _networkUsageViewModel = new NetworkUsageViewModel(monitorService);
                Debug.WriteLine("Created new NetworkUsageViewModel with NetworkMonitorService");

                var usageWindow = new NetworkUsageWindow();
                usageWindow.Owner = this;
                usageWindow.DataContext = _networkUsageViewModel;
                Debug.WriteLine("Set NetworkUsageWindow DataContext");

                usageWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing NetworkUsageWindow: {ex.Message}");
                MessageBox.Show($"Error showing network usage window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
                Debug.WriteLine("Window hidden");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error hiding window: {ex.Message}");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Debug.WriteLine("Window source initialized");
        }

        protected override void OnClosed(System.EventArgs e)
        {
            try
            {
                _networkUsageViewModel?.Dispose();
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
                base.OnClosed(e);
                Debug.WriteLine("Window closed and NotifyIcon disposed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnClosed: {ex.Message}");
            }
        }
    }
}