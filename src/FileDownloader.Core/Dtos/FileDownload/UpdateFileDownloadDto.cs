using System.ComponentModel.DataAnnotations;

namespace FileDownloader.Core.Dtos.FileDownload
{
    public class UpdateFileDownloadDto
    {
        [Required]
        public string FileStatus { get; set; }
    }
}