using FileDownloader.Core.Dtos.FileDownload;
using FileDownloader.Core.Entities;
using FileDownloader.Core.Exceptions;
using FileDownloader.Core.Helpers;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.Settings;
using FileDownloader.Core.Utils;
using FileDownloader.Core.ViewModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Core.Services
{
    public class FileDownloadService : IFileDownloadService
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationSettings _applicationSettings;
        private readonly FileHelper _fileHelper;

        public FileDownloadService(IUnitOfWork uow,
            IOptions<ApplicationSettings> applicationSettings,
            FileHelper fileHelper)
        {
            _uow = uow;
            _applicationSettings = applicationSettings.Value;
            _fileHelper = fileHelper;
        }

        public async Task<List<FileDownloadViewModel>> Get(CancellationToken ct)
        {
            List<FileDownload> fileDownloads = await _uow.GetRepository<FileDownload>().GetAll(ct);

            List<FileDownloadViewModel> fileDownloadsViewModel = Map(fileDownloads);

            return fileDownloadsViewModel;
        }

        public async Task UpdateFileDownloadStatus(int id, UpdateFileDownloadDto updateFileDownloadDto, CancellationToken ct)
        {
            FileDownload fileDownload = await _uow.GetRepository<FileDownload>().Get(id, ct);

            if (fileDownload == null)
                throw new CustomException(Constants.NOT_FOUND, Constants.NOT_FOUND_MSG, HttpStatusCode.NotFound);

            if (updateFileDownloadDto.FileStatus.Equals(Constants.ACTION_ACCEPTED, StringComparison.OrdinalIgnoreCase))
            {
                fileDownload.Status = (int)FileStatus.Accepted;
            }
            else if (updateFileDownloadDto.FileStatus.Equals(Constants.ACTION_REJECTED, StringComparison.OrdinalIgnoreCase))
            {
                fileDownload.Status = (int)FileStatus.Rejected;
            }
            else
            {
                throw new CustomException(Constants.UNKNOWN_ACTION, Constants.UNKNOWN_ACTION_MSG, HttpStatusCode.BadRequest);
            }

            await _uow.Commit(ct);
        }

        private List<FileDownloadViewModel> Map(List<FileDownload> fileDownloads)
        {
            List<FileDownloadViewModel> fileDownloadsViewModel = new List<FileDownloadViewModel>();

            FileDownloadViewModel fileDownloadViewModel = new FileDownloadViewModel();

            if (fileDownloads != null)
            {
                string Url = _applicationSettings.Url;
                string sourcePath = _applicationSettings.DestinationFolder;

                foreach (FileDownload fileDownload in fileDownloads)
                {
                    fileDownloadViewModel = new FileDownloadViewModel()
                    {
                        FileName = fileDownload.FileName,
                        Status = Enum.GetName(typeof(FileStatus), fileDownload.Status),
                        Url = $"{Url}/{fileDownload.FileName}",
                        Id = fileDownload.Id
                    };

                    FileType? fileType = _fileHelper.GetFileType($"{sourcePath}/{fileDownload.FileName}");
                    if (fileType.HasValue)
                    {
                        fileDownloadViewModel.FileType = fileType.ToString();
                    }

                    fileDownloadsViewModel.Add(fileDownloadViewModel);
                }
            }

            return fileDownloadsViewModel;
        }
    }
}