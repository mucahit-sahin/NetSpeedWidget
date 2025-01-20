using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using NetSpeedWidget.ViewModels;
using NetSpeedWidget.Services;
using NetSpeedWidget.Views;
using System.Diagnostics;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using Icon = System.Drawing.Icon;

namespace NetSpeedWidget.Views
{
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly MainViewModel _viewModel;
        private NetworkUsageViewModel? _networkUsageViewModel;
        private NetworkUsageWindow? _networkUsageWindow;
        private SettingsWindow? _settingsWindow;
        private NetworkStatsWindow? _networkStatsWindow;

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
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "assets", "image.ico");
                Debug.WriteLine($"Loading icon from: {iconPath}");

                _notifyIcon = new NotifyIcon
                {
                    Icon = new Icon(iconPath),
                    Visible = true,
                    Text = "Network Speed Widget"
                };
                Debug.WriteLine("NotifyIcon created");

                // Create context menu
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
                contextMenu.Items.Add("Network Stats", null, (s, e) => ShowNetworkStatsWindow());
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
            else if (e.ClickCount == 3)  // Triple click to show stats
            {
                ShowNetworkStatsWindow();
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
                // If window exists, just activate it
                if (_networkUsageWindow != null)
                {
                    _networkUsageWindow.Activate();
                    Debug.WriteLine("Existing NetworkUsageWindow activated");
                    return;
                }

                // Use the existing NetworkMonitorService from MainViewModel
                _networkUsageViewModel = new NetworkUsageViewModel(_viewModel.NetworkMonitorService);
                Debug.WriteLine("Created new NetworkUsageViewModel with existing NetworkMonitorService");

                _networkUsageWindow = new NetworkUsageWindow();
                _networkUsageWindow.Owner = this;
                _networkUsageWindow.DataContext = _networkUsageViewModel;
                Debug.WriteLine("Set NetworkUsageWindow DataContext");

                // Clear the reference when the window is closed
                _networkUsageWindow.Closed += (s, e) =>
                {
                    _networkUsageWindow = null;
                    _networkUsageViewModel = null;
                    Debug.WriteLine("NetworkUsageWindow reference and ViewModel cleared");
                };

                _networkUsageWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing NetworkUsageWindow: {ex.Message}");
                MessageBox.Show($"Error showing network usage window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowNetworkStatsWindow()
        {
            try
            {
                // If window exists, just activate it
                if (_networkStatsWindow != null)
                {
                    _networkStatsWindow.Activate();
                    Debug.WriteLine("Existing NetworkStatsWindow activated");
                    return;
                }

                _networkStatsWindow = new NetworkStatsWindow();
                _networkStatsWindow.Owner = this;

                // Clear the reference when the window is closed
                _networkStatsWindow.Closed += (s, e) =>
                {
                    _networkStatsWindow = null;
                    Debug.WriteLine("NetworkStatsWindow reference cleared");
                };

                _networkStatsWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing NetworkStatsWindow: {ex.Message}");
                MessageBox.Show($"Error showing network stats window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // If window exists, just activate it
                if (_settingsWindow != null)
                {
                    _settingsWindow.Activate();
                    Debug.WriteLine("Existing SettingsWindow activated");
                    return;
                }

                _settingsWindow = new SettingsWindow();
                _settingsWindow.Owner = this;

                // Clear the reference when the window is closed
                _settingsWindow.Closed += (s, args) =>
                {
                    _settingsWindow = null;
                    Debug.WriteLine("SettingsWindow reference cleared");
                };

                _settingsWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing SettingsWindow: {ex.Message}");
                MessageBox.Show($"Error showing settings window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNetworkStatsWindow();
        }

        private void NetworkUsageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNetworkUsageWindow();
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