using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using NetSpeedWidget.Models;
using NetSpeedWidget.Services;

namespace NetSpeedWidget.ViewModels
{
    public partial class NetworkUsageViewModel : ObservableObject, IDisposable
    {
        private readonly NetworkMonitorService _networkMonitor;
        private readonly Dispatcher _dispatcher;
        private readonly bool _ownsMonitorService;

        [ObservableProperty]
        private ObservableCollection<NetworkUsageInfo> _networkUsages = new();

        public NetworkMonitorService NetworkMonitorService => _networkMonitor;

        public NetworkUsageViewModel(NetworkMonitorService? existingMonitorService = null)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            if (existingMonitorService != null)
            {
                _networkMonitor = existingMonitorService;
                _ownsMonitorService = false;
            }
            else
            {
                _networkMonitor = new NetworkMonitorService();
                _ownsMonitorService = true;
            }

            _networkMonitor.StatsUpdated += NetworkMonitor_StatsUpdated;
            Debug.WriteLine("NetworkUsageViewModel initialized with monitor service");
        }

        private void NetworkMonitor_StatsUpdated(object? sender, System.Collections.Generic.Dictionary<int, NetworkUsageInfo> stats)
        {
            try
            {
                var orderedStats = stats.Values
                    .Where(info => info.TotalBytesPerSecond > 0)
                    .OrderByDescending(info => info.TotalBytesPerSecond)
                    .Take(20)
                    .ToList();

                _dispatcher.BeginInvoke(() =>
                {
                    NetworkUsages.Clear();
                    foreach (var stat in orderedStats)
                    {
                        NetworkUsages.Add(stat);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating network usages: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_ownsMonitorService)
            {
                _networkMonitor.Dispose();
            }
        }
    }
}