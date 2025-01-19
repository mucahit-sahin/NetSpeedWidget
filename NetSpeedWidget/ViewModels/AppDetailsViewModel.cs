using System;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using NetSpeedWidget.Models;
using NetSpeedWidget.Services;

namespace NetSpeedWidget.ViewModels
{
    public partial class AppDetailsViewModel : ObservableObject, IDisposable
    {
        private readonly NetworkMonitorService _networkMonitorService;
        private readonly int _processId;

        [ObservableProperty]
        private NetworkUsageInfo _selectedApp;

        public AppDetailsViewModel(NetworkUsageInfo selectedApp, NetworkMonitorService networkMonitorService)
        {
            _selectedApp = selectedApp;
            _processId = selectedApp.ProcessId;
            _networkMonitorService = networkMonitorService;

            // Subscribe to network stats updates
            _networkMonitorService.StatsUpdated += NetworkMonitorService_StatsUpdated;
        }

        private void NetworkMonitorService_StatsUpdated(object? sender, System.Collections.Generic.Dictionary<int, NetworkUsageInfo> stats)
        {
            if (stats.TryGetValue(_processId, out var updatedStats))
            {
                // Update the stats while preserving the icon
                updatedStats.Icon = SelectedApp.Icon;
                SelectedApp = updatedStats;
            }
        }

        public void Dispose()
        {
            _networkMonitorService.StatsUpdated -= NetworkMonitorService_StatsUpdated;
        }
    }
}