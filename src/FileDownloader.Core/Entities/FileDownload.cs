using FileDownloader.Core.SharedKernel;

namespace FileDownloader.Core.Entities
{
    public class FileDownload : BaseEntity
    {
        public string FileName { get; set; }
        public int Status { get; set; }
    }
}