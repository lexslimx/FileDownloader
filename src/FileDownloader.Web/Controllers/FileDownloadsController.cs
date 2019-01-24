using FileDownloader.Core.Dtos.FileDownload;
using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Web.Controllers
{
    public class FileDownloadsController : BaseApiController
    {
        private readonly IFileDownloadService _fileDownloadService;

        public FileDownloadsController(IFileDownloadService fileDownloadService)
        {
            _fileDownloadService = fileDownloadService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetFileDownloads(CancellationToken ct)
        {
            List<FileDownloadViewModel> fileDownloads = await _fileDownloadService.Get(ct);

            return Ok(fileDownloads);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateFileDownloadStatus(int id, [FromBody]  UpdateFileDownloadDto updateFileDownloadDto, CancellationToken ct)
        {
            await _fileDownloadService.UpdateFileDownloadStatus(id, updateFileDownloadDto, ct);

            return Ok();
        }
    }
}