namespace FileDownloader.Core.ViewModels
{
    public class FileDownloadViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public string FileType { get; set; }
    }
}