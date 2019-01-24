using FileDownloader.Core.Entities;
using FileDownloader.Core.Helpers;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.Models;
using FileDownloader.Core.Settings;
using FileDownloader.Core.Utils;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Core.Services
{
    public class FtpFileDownloader : IDownloader
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationSettings _applicationSettings;
        private readonly FileHelper _fileHelper;

        public FtpFileDownloader(IUnitOfWork uow,
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

                string fileName = Path.GetFileName(uri.LocalPath); ;

                NetworkCredential credentials = new NetworkCredential(fileDownloadRequest.NetworkCredentials.Username, fileDownloadRequest.NetworkCredentials.Password);

                responseLength = await GetContentLengthAsync(uri, credentials);

                FileInfo file = _fileHelper.SetupFile(uri, destinationFolder);
                destinationFilePath = file.FullName;

                await Download(uri, credentials, file);

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

        private async Task<long> GetContentLengthAsync(Uri uri, NetworkCredential networkCredential)
        {
            long responseLength;

            WebRequest sizeRequest = WebRequest.Create(uri.ToString());

            sizeRequest.Credentials = networkCredential;
            sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;

            using (WebResponse response = await sizeRequest.GetResponseAsync())
            {
                responseLength = (int)response.ContentLength;
            }

            return responseLength;
        }

        private async Task Download(Uri uri, NetworkCredential networkCredential, FileInfo fileInfo)
        {
            WebRequest request = WebRequest.Create(uri.ToString());
            request.Credentials = networkCredential;
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (Stream ftpStream = (await request.GetResponseAsync()).GetResponseStream())
            using (Stream fileStream = fileInfo.Create())
            {
                // download in blocks of 4kb each
                byte[] buffer = new byte[4096];
                int read;
                while ((read = await ftpStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    int position = (int)fileStream.Position;
                }
            }
        }
    }
}