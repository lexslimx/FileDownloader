using FileDownloader.Core.Models;
using System.Threading.Tasks;

namespace FileDownloader.Core.Interfaces.Services
{
    public interface IDownloader
    {
        Task<DownloadResult> DownloadAsync(FileDownloadRequest fileDownloadRequest);
    }
}