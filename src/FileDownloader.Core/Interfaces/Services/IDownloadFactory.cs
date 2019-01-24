using FileDownloader.Core.Models;
using System.Threading.Tasks;

namespace FileDownloader.Core.Services
{
    public interface IDownloadCreator
    {
        Task<DownloadResult> DownloadAsync(FileDownloadRequest fileDownloadRequest);
    }
}