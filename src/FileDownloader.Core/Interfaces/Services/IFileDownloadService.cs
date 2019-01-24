using FileDownloader.Core.Dtos.FileDownload;
using FileDownloader.Core.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Core.Interfaces.Services
{
    public interface IFileDownloadService
    {
        Task<List<FileDownloadViewModel>> Get(CancellationToken ct);

        Task UpdateFileDownloadStatus(int id, UpdateFileDownloadDto updateFileDownloadDto, CancellationToken ct);
    }
}