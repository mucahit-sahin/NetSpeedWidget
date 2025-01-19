using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using NetSpeedWidget.Models;
using NetSpeedWidget.ViewModels;
using NetSpeedWidget.Services;
using NetSpeedWidget.Views;
using System.Diagnostics;
using System.Collections.Generic;

namespace NetSpeedWidget.Views
{
    public partial class NetworkUsageWindow : Window
    {
        private NetworkUsageViewModel? _viewModel;
        private readonly Dictionary<int, AppDetailsWindow> _openDetailWindows = new();

        public NetworkUsageWindow()
        {
            InitializeComponent();
            Debug.WriteLine("NetworkUsageWindow initialized");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _viewModel = DataContext as NetworkUsageViewModel;
            Debug.WriteLine($"DataContext type: {DataContext?.GetType().FullName}");
            Debug.WriteLine($"ViewModel is null: {_viewModel == null}");

            if (_viewModel != null)
            {
                Debug.WriteLine($"NetworkMonitorService is null: {_viewModel.NetworkMonitorService == null}");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("ListView_MouseDoubleClick triggered");

            if (sender is ListView listView)
            {
                Debug.WriteLine($"ListView found, SelectedItem: {listView.SelectedItem != null}");

                if (listView.SelectedItem is NetworkUsageInfo selectedApp)
                {
                    Debug.WriteLine($"Selected app: {selectedApp.ProcessName}");

                    // Re-get the ViewModel from DataContext in case it changed
                    _viewModel = DataContext as NetworkUsageViewModel;
                    Debug.WriteLine($"ViewModel is null: {_viewModel == null}");

                    if (_viewModel?.NetworkMonitorService != null)
                    {
                        // Check if window already exists for this process
                        if (_openDetailWindows.TryGetValue(selectedApp.ProcessId, out var existingWindow))
                        {
                            Debug.WriteLine($"Window already exists for process {selectedApp.ProcessId}");
                            existingWindow.Activate(); // Bring existing window to front
                            return;
                        }

                        Debug.WriteLine("NetworkMonitorService is available");
                        var detailsWindow = new AppDetailsWindow(selectedApp, _viewModel.NetworkMonitorService);
                        detailsWindow.Owner = Application.Current.MainWindow;

                        // Add to tracking dictionary
                        _openDetailWindows[selectedApp.ProcessId] = detailsWindow;

                        // Remove from tracking when closed
                        detailsWindow.Closed += (s, args) =>
                        {
                            _openDetailWindows.Remove(selectedApp.ProcessId);
                            Debug.WriteLine($"Removed window tracking for process {selectedApp.ProcessId}");
                        };

                        detailsWindow.Show();
                        Debug.WriteLine("AppDetailsWindow shown");
                    }
                    else
                    {
                        Debug.WriteLine($"NetworkMonitorService is null. ViewModel: {_viewModel != null}");
                    }
                }
                else
                {
                    Debug.WriteLine("SelectedItem is not NetworkUsageInfo");
                }
            }
            else
            {
                Debug.WriteLine("Sender is not ListView");
            }
        }
    }
}