using System;
using System.Collections.Generic;

namespace NetSpeedWidget.Models
{
    public class NetworkUsageData
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public double TotalDownloadBytes { get; set; }
        public double TotalUploadBytes { get; set; }
        public List<ApplicationUsageData> ApplicationUsages { get; set; } = new();
    }

    public class ApplicationUsageData
    {
        public int Id { get; set; }
        public int NetworkUsageDataId { get; set; }
        public NetworkUsageData NetworkUsageData { get; set; }
        public string ProcessName { get; set; }
        public double DownloadBytes { get; set; }
        public double UploadBytes { get; set; }
    }
}