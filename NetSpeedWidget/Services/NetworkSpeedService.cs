using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NetSpeedWidget.Models;
using System.Windows;

namespace NetSpeedWidget.Services
{
    public class NetworkSpeedService
    {
        private PerformanceCounter? downloadCounter;
        private PerformanceCounter? uploadCounter;
        private string? selectedInterface;
        private bool isInitialized;

        public NetworkSpeedService()
        {
            try
            {
                InitializeCounters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Network monitoring initialization failed: {ex.Message}\n\nPlease run the application as administrator.",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void InitializeCounters()
        {
            try
            {
                // Get all network interfaces
                var category = new PerformanceCounterCategory("Network Interface");
                var interfaces = category.GetInstanceNames();

                Debug.WriteLine($"Found {interfaces.Length} network interfaces");
                foreach (var iface in interfaces)
                {
                    Debug.WriteLine($"Interface: {iface}");
                }

                // Try to find the most active interface
                foreach (var iface in interfaces)
                {
                    if (iface.Contains("Loopback", StringComparison.OrdinalIgnoreCase) ||
                        iface.Contains("isatap", StringComparison.OrdinalIgnoreCase) ||
                        iface.Contains("Teredo", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        var tempDownCounter = new PerformanceCounter("Network Interface",
                            "Bytes Received/sec", iface, true);
                        var tempUpCounter = new PerformanceCounter("Network Interface",
                            "Bytes Sent/sec", iface, true);

                        // Read initial values
                        tempDownCounter.NextValue();
                        tempUpCounter.NextValue();

                        // Wait a bit and read again
                        System.Threading.Thread.Sleep(100);
                        var down = tempDownCounter.NextValue();
                        var up = tempUpCounter.NextValue();

                        Debug.WriteLine($"Testing interface {iface} - Down: {down}, Up: {up}");

                        // If we get any activity or this is the first valid interface
                        if (down > 0 || up > 0 || selectedInterface == null)
                        {
                            // Cleanup previous counters if they exist
                            downloadCounter?.Dispose();
                            uploadCounter?.Dispose();

                            // Set new interface and counters
                            selectedInterface = iface;
                            downloadCounter = tempDownCounter;
                            uploadCounter = tempUpCounter;

                            Debug.WriteLine($"Selected interface: {selectedInterface}");

                            if (down > 0 || up > 0)
                            {
                                // If we found an active interface, stop looking
                                break;
                            }
                        }
                        else
                        {
                            // Cleanup temp counters if we don't use them
                            tempDownCounter.Dispose();
                            tempUpCounter.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error testing interface {iface}: {ex.Message}");
                    }
                }

                if (selectedInterface != null && downloadCounter != null && uploadCounter != null)
                {
                    // Initialize counters one more time
                    downloadCounter.NextValue();
                    uploadCounter.NextValue();
                    isInitialized = true;

                    Debug.WriteLine($"Successfully initialized with interface: {selectedInterface}");
                }
                else
                {
                    MessageBox.Show("No active network interface found.",
                        "Initialization Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied. Please run the application as administrator.",
                    "Permission Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize network monitoring: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        public NetworkSpeed GetCurrentSpeed()
        {
            if (!isInitialized || downloadCounter == null || uploadCounter == null)
                return new NetworkSpeed();

            try
            {
                // Read values twice to get current speed
                downloadCounter.NextValue();
                uploadCounter.NextValue();

                System.Threading.Thread.Sleep(100);

                var downloadSpeed = downloadCounter.NextValue();
                var uploadSpeed = uploadCounter.NextValue();

                Debug.WriteLine($"Current speeds - Download: {downloadSpeed}, Upload: {uploadSpeed}");

                return new NetworkSpeed
                {
                    DownloadSpeed = downloadSpeed,
                    UploadSpeed = uploadSpeed
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting network speed: {ex.Message}");
                return new NetworkSpeed();
            }
        }

        public void Dispose()
        {
            try
            {
                downloadCounter?.Dispose();
                uploadCounter?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing counters: {ex.Message}");
            }
        }
    }
}