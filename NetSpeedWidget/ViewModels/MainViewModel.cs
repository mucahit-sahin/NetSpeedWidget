using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetSpeedWidget.Models;
using NetSpeedWidget.Services;
using System.Windows;
using System.Diagnostics;

namespace NetSpeedWidget.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly NetworkSpeedService _networkSpeedService = null!;
        private readonly NetworkMonitorService _networkMonitorService = null!;
        private readonly DispatcherTimer _timer = null!;

        public NetworkMonitorService NetworkMonitorService => _networkMonitorService;

        [ObservableProperty]
        private NetworkSpeed _networkSpeed = new();

        [ObservableProperty]
        private string _status = "Initializing...";

        [ObservableProperty]
        private bool _isMonitoring;

        public MainViewModel()
        {
            try
            {
                Debug.WriteLine("Initializing MainViewModel");
                _networkSpeedService = new NetworkSpeedService();
                _networkMonitorService = new NetworkMonitorService();
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1500)
                };
                _timer.Tick += Timer_Tick;
                StartMonitoring();
                Debug.WriteLine("MainViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                Status = "Failed to initialize network monitoring.";
                Debug.WriteLine($"Error in MainViewModel constructor: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void StartMonitoring()
        {
            if (!IsMonitoring)
            {
                try
                {
                    _timer.Start();
                    IsMonitoring = true;
                    Status = "Monitoring network speed...";
                    Debug.WriteLine("Network monitoring started");
                }
                catch (Exception ex)
                {
                    Status = "Failed to start monitoring.";
                    Debug.WriteLine($"Error starting monitoring: {ex.Message}");
                }
            }
        }

        private void StopMonitoring()
        {
            if (IsMonitoring)
            {
                try
                {
                    _timer.Stop();
                    IsMonitoring = false;
                    Status = "Monitoring stopped.";
                    Debug.WriteLine("Network monitoring stopped");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping monitoring: {ex.Message}");
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var oldSpeed = NetworkSpeed;
                NetworkSpeed = _networkSpeedService.GetCurrentSpeed();

                if (NetworkSpeed.DownloadSpeed == 0 && NetworkSpeed.UploadSpeed == 0)
                {
                    Debug.WriteLine("Warning: Both download and upload speeds are 0");
                }

                Debug.WriteLine($"Speed updated - Download: {NetworkSpeed.DownloadSpeedFormatted}, Upload: {NetworkSpeed.UploadSpeedFormatted}");
            }
            catch (Exception ex)
            {
                StopMonitoring();
                Status = "Error occurred while monitoring network speed.";
                Debug.WriteLine($"Error in Timer_Tick: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Monitoring Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Exit()
        {
            try
            {
                Debug.WriteLine("Shutting down application");
                StopMonitoring();
                _networkSpeedService.Dispose();
                _networkMonitorService.Dispose();
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during shutdown: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ShowWindow()
        {
            try
            {
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.Activate();
                    Debug.WriteLine("Main window shown and activated");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing window: {ex.Message}");
            }
        }
    }
}