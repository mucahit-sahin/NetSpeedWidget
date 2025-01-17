using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Session;
using NetSpeedWidget.Models;

namespace NetSpeedWidget.Services
{
    public class NetworkMonitorService : IDisposable
    {
        private readonly ConcurrentDictionary<int, NetworkUsageInfo> _processStats = new();
        private TraceEventSession? _session;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task? _monitoringTask;

        public event EventHandler<Dictionary<int, NetworkUsageInfo>>? StatsUpdated;

        public NetworkMonitorService()
        {
            StartMonitoring();
        }

        private void StartMonitoring()
        {
            try
            {
                _monitoringTask = Task.Run(() =>
                {
                    using (_session = new TraceEventSession("NetSpeedWidgetSession"))
                    {
                        _session.EnableKernelProvider(
                            Microsoft.Diagnostics.Tracing.Parsers.KernelTraceEventParser.Keywords.NetworkTCPIP);

                        _session.Source.Kernel.TcpIpRecv += data =>
                        {
                            try
                            {
                                var processId = data.ProcessID;
                                var bytes = data.size;

                                _processStats.AddOrUpdate(
                                    processId,
                                    id => new NetworkUsageInfo
                                    {
                                        ProcessId = id,
                                        ProcessName = GetProcessName(id),
                                        DownloadBytesPerSecond = bytes
                                    },
                                    (id, existing) =>
                                    {
                                        existing.DownloadBytesPerSecond += bytes;
                                        return existing;
                                    });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error in TcpIpRecv: {ex.Message}");
                            }
                        };

                        _session.Source.Kernel.TcpIpSend += data =>
                        {
                            try
                            {
                                var processId = data.ProcessID;
                                var bytes = data.size;

                                _processStats.AddOrUpdate(
                                    processId,
                                    id => new NetworkUsageInfo
                                    {
                                        ProcessId = id,
                                        ProcessName = GetProcessName(id),
                                        UploadBytesPerSecond = bytes
                                    },
                                    (id, existing) =>
                                    {
                                        existing.UploadBytesPerSecond += bytes;
                                        return existing;
                                    });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error in TcpIpSend: {ex.Message}");
                            }
                        };

                        // Reset stats every second
                        var resetTimer = new Timer(_ =>
                        {
                            try
                            {
                                var currentStats = new Dictionary<int, NetworkUsageInfo>();
                                foreach (var kvp in _processStats)
                                {
                                    currentStats[kvp.Key] = new NetworkUsageInfo
                                    {
                                        ProcessId = kvp.Key,
                                        ProcessName = kvp.Value.ProcessName,
                                        DownloadBytesPerSecond = kvp.Value.DownloadBytesPerSecond,
                                        UploadBytesPerSecond = kvp.Value.UploadBytesPerSecond
                                    };
                                }

                                StatsUpdated?.Invoke(this, currentStats);

                                // Reset counters
                                foreach (var kvp in _processStats)
                                {
                                    kvp.Value.DownloadBytesPerSecond = 0;
                                    kvp.Value.UploadBytesPerSecond = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error in reset timer: {ex.Message}");
                            }
                        }, null, 0, 1000);

                        _session.Source.Process();
                    }
                }, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting network monitoring: {ex.Message}");
            }
        }

        private string GetProcessName(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                return process.ProcessName;
            }
            catch
            {
                return $"Process {processId}";
            }
        }

        public void Dispose()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _session?.Dispose();
                _monitoringTask?.Wait();
                _cancellationTokenSource.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing NetworkMonitorService: {ex.Message}");
            }
        }
    }
}