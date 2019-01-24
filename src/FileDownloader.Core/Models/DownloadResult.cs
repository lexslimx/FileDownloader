using System;

namespace FileDownloader.Core.Models
{
    public class DownloadResult
    {
        public long Size { get; set; }
        public string FilePath { get; set; }
        public TimeSpan TimeElapsed { get; set; }
        public int ConcurrentDownloads { get; set; }
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
    }
}