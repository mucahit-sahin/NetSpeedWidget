using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using NetSpeedWidget.Services;

namespace NetSpeedWidget.ViewModels
{
    public partial class NetworkStatsViewModel : ObservableObject
    {
        private readonly NetworkStatsService _statsService;
        private readonly Dispatcher _dispatcher;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private string _selectedPeriod = "Daily";

        [ObservableProperty]
        private ObservableCollection<ISeries> _downloadSeries = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> _uploadSeries = new();

        [ObservableProperty]
        private ObservableCollection<AppUsageStats> _applicationStats = new();

        [ObservableProperty]
        private ObservableCollection<Axis> _xAxes;

        [ObservableProperty]
        private ObservableCollection<Axis> _yAxes;

        [ObservableProperty]
        private string[] _xLabels = Array.Empty<string>();

        public ObservableCollection<string> Periods { get; } = new()
        {
            "Daily",
            "Weekly",
            "Monthly",
            "Yearly"
        };

        public NetworkStatsViewModel()
        {
            _statsService = new NetworkStatsService();
            _dispatcher = Dispatcher.CurrentDispatcher;

            // Initialize axes
            _xAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "Time",
                    TextSize = 12,
                    NameTextSize = 14,
                    ShowSeparatorLines = false,
                    LabelsPaint = new SolidColorPaint { Color = SKColors.LightGray },
                    NamePaint = new SolidColorPaint { Color = SKColors.LightGray }
                }
            };

            _yAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "MB/s",
                    TextSize = 12,
                    NameTextSize = 14,
                    ShowSeparatorLines = false,
                    LabelsPaint = new SolidColorPaint { Color = SKColors.LightGray },
                    NamePaint = new SolidColorPaint { Color = SKColors.LightGray }
                }
            };

            UpdateStatsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task UpdateStats()
        {
            try
            {
                var (startDate, endDate) = GetDateRange();
                var stats = SelectedPeriod switch
                {
                    "Daily" => await _statsService.GetDailyStatsAsync(SelectedDate),
                    "Weekly" => await _statsService.GetWeeklyStatsAsync(startDate),
                    "Monthly" => await _statsService.GetMonthlyStatsAsync(SelectedDate.Year, SelectedDate.Month),
                    "Yearly" => await _statsService.GetYearlyStatsAsync(SelectedDate.Year),
                    _ => throw new ArgumentException("Invalid period")
                };

                var appTotals = await _statsService.GetApplicationTotalsAsync(startDate, endDate);

                _dispatcher.Invoke(() =>
                {
                    // Group data by hour/day/month based on selected period
                    var groupedStats = SelectedPeriod switch
                    {
                        "Daily" => stats.GroupBy(s => s.Timestamp.Hour)
                                      .Select(g => new
                                      {
                                          Time = g.Key.ToString("00") + ":00",
                                          Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0), // MB
                                          Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0) // MB
                                      })
                                      .OrderBy(x => x.Time),
                        "Weekly" => stats.GroupBy(s => s.Timestamp.Date)
                                       .Select(g => new
                                       {
                                           Time = g.Key.ToString("MM/dd"),
                                           Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0),
                                           Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0)
                                       })
                                       .OrderBy(x => x.Time),
                        "Monthly" => stats.GroupBy(s => s.Timestamp.Date)
                                        .Select(g => new
                                        {
                                            Time = g.Key.ToString("MM/dd"),
                                            Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0),
                                            Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0)
                                        })
                                        .OrderBy(x => x.Time),
                        "Yearly" => stats.GroupBy(s => s.Timestamp.Month)
                                       .Select(g => new
                                       {
                                           Time = new DateTime(2000, g.Key, 1).ToString("MMMM"),
                                           Download = g.Sum(x => x.TotalDownloadBytes) / (1024.0 * 1024.0),
                                           Upload = g.Sum(x => x.TotalUploadBytes) / (1024.0 * 1024.0)
                                       })
                                       .OrderBy(x => x.Time),
                        _ => throw new ArgumentException("Invalid period")
                    };

                    var data = groupedStats.ToList();
                    var timeLabels = data.Select(x => x.Time).ToArray();

                    // Update X axis labels
                    XAxes[0].Labels = timeLabels;

                    // Create download series
                    var downloadSeries = new LineSeries<double>
                    {
                        Values = data.Select(x => x.Download).ToList(),
                        Name = "Download",
                        Stroke = new SolidColorPaint(SKColors.LightGreen, 2),
                        GeometrySize = 8,
                        GeometryStroke = new SolidColorPaint(SKColors.LightGreen, 2),
                        GeometryFill = new SolidColorPaint(SKColors.DarkGreen),
                        Fill = null
                    };

                    // Create upload series
                    var uploadSeries = new LineSeries<double>
                    {
                        Values = data.Select(x => x.Upload).ToList(),
                        Name = "Upload",
                        Stroke = new SolidColorPaint(SKColors.Orange, 2),
                        GeometrySize = 8,
                        GeometryStroke = new SolidColorPaint(SKColors.Orange, 2),
                        GeometryFill = new SolidColorPaint(SKColors.DarkOrange),
                        Fill = null
                    };

                    // Update chart series
                    DownloadSeries = new ObservableCollection<ISeries> { downloadSeries };
                    UploadSeries = new ObservableCollection<ISeries> { uploadSeries };

                    // Update application stats
                    ApplicationStats = new ObservableCollection<AppUsageStats>(
                        appTotals.Select(kvp => new AppUsageStats
                        {
                            ProcessName = kvp.Key,
                            TotalDownload = FormatBytes(kvp.Value.TotalDownload),
                            TotalUpload = FormatBytes(kvp.Value.TotalUpload)
                        })
                        .OrderByDescending(x => ParseBytes(x.TotalDownload)));
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error updating stats: {ex.Message}");
            }
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            return SelectedPeriod switch
            {
                "Daily" => (SelectedDate.Date, SelectedDate.Date.AddDays(1)),
                "Weekly" => (SelectedDate.Date.AddDays(-(int)SelectedDate.DayOfWeek), SelectedDate.Date.AddDays(7 - (int)SelectedDate.DayOfWeek)),
                "Monthly" => (new DateTime(SelectedDate.Year, SelectedDate.Month, 1), new DateTime(SelectedDate.Year, SelectedDate.Month, 1).AddMonths(1)),
                "Yearly" => (new DateTime(SelectedDate.Year, 1, 1), new DateTime(SelectedDate.Year + 1, 1, 1)),
                _ => throw new ArgumentException("Invalid period")
            };
        }

        private string FormatBytes(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }

        private double ParseBytes(string formattedBytes)
        {
            var parts = formattedBytes.Split(' ');
            if (parts.Length != 2 || !double.TryParse(parts[0], out double value))
                return 0;

            return parts[1] switch
            {
                "KB" => value * 1024,
                "MB" => value * 1024 * 1024,
                "GB" => value * 1024 * 1024 * 1024,
                "TB" => value * 1024 * 1024 * 1024 * 1024,
                _ => value
            };
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            UpdateStatsCommand.Execute(null);
        }

        partial void OnSelectedPeriodChanged(string value)
        {
            UpdateStatsCommand.Execute(null);
        }
    }

    public class AppUsageStats
    {
        public string ProcessName { get; set; }
        public string TotalDownload { get; set; }
        public string TotalUpload { get; set; }
    }
}