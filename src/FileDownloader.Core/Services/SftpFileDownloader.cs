using FileDownloader.Core.Entities;
using FileDownloader.Core.Helpers;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.Models;
using FileDownloader.Core.Settings;
using FileDownloader.Core.Utils;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Core.Services
{
    public class SftpFileDownloader : IDownloader
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationSettings _applicationSettings;
        private readonly FileHelper _fileHelper;

        public SftpFileDownloader(IUnitOfWork uow,
            IOptions<ApplicationSettings> applicationSettings,
            FileHelper fileHelper)
        {
            _uow = uow;
            _applicationSettings = applicationSettings.Value;
            _fileHelper = fileHelper;
        }

        public async Task<DownloadResult> DownloadAsync(FileDownloadRequest fileDownloadRequest)
        {
            int nbOfConcurrentDownloads = 1;
            string destinationFolder = _applicationSettings.DestinationFolder;

            long responseLength = 0;
            string destinationFilePath = null;
            bool isSuccess = false;
            TimeSpan timeElapsed;
            string error = null;

            DownloadResult result = new DownloadResult();

            DateTime startTime = DateTime.UtcNow;

            try
            {
                Uri uri = fileDownloadRequest.ResourceLink;

                string host = uri.Host;
                string username = fileDownloadRequest.NetworkCredentials.Username;
                string password = fileDownloadRequest.NetworkCredentials.Password;

                string pathRemoteFile = uri.PathAndQuery;

                FileInfo file = _fileHelper.SetupFile(uri, destinationFolder);
                destinationFilePath = file.FullName;

                Download(host, username, password, file.FullName, pathRemoteFile, ref responseLength);

                isSuccess = true;
                timeElapsed = DateTime.UtcNow.Subtract(startTime);

                CancellationToken ct = new CancellationToken();

                await _uow.GetRepository<FileDownload>().Add(new FileDownload()
                {
                    FileName = file.Name,
                    Status = (int)FileStatus.Ready
                }, ct);

                await _uow.Commit(ct);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                timeElapsed = DateTime.UtcNow.Subtract(startTime);
                error = ex.Message;
            }

            result.ConcurrentDownloads = nbOfConcurrentDownloads;
            result.IsSuccess = isSuccess;
            result.Size = responseLength;
            result.FilePath = destinationFilePath;
            result.TimeElapsed = timeElapsed;
            result.Error = error;

            return result;
        }

        private void Download(string host, string username, string password, string pathLocalFile, string pathRemoteFile, ref long size)
        {
            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                sftp.Connect();

                using (Stream fileStream = File.OpenWrite(pathLocalFile))
                {
                    sftp.DownloadFile(pathRemoteFile, fileStream);

                    size = fileStream.Length;
                }

                sftp.Disconnect();
            }
        }
    }
}