using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.Models;
using FileDownloader.Core.Utils;
using System;
using System.Threading.Tasks;

namespace FileDownloader.Core.Services
{
    public class DownloadCreator : IDownloadCreator
    {
        private readonly FtpFileDownloader _ftp;
        private readonly HttpFileDownloader _http;
        private readonly SftpFileDownloader _sftp;

        public DownloadCreator(FtpFileDownloader ftp,
            HttpFileDownloader http,
            SftpFileDownloader sftp)
        {
            _ftp = ftp;
            _http = http;
            _sftp = sftp;
        }

        public async Task<DownloadResult> DownloadAsync(FileDownloadRequest fileDownloadRequest)
        {
            DownloadResult result = new DownloadResult();

            try
            {
                result = await Create(fileDownloadRequest.ResourceLink).DownloadAsync(fileDownloadRequest);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Error = ex.Message;
            }

            return result;
        }

        public IDownloader Create(Uri resourceLink)
        {
            IDownloader downloader = null;

            if (resourceLink.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                resourceLink.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                downloader = _http;
            }
            else if (resourceLink.Scheme.Equals(Uri.UriSchemeFtp, StringComparison.OrdinalIgnoreCase))
            {
                downloader = _ftp;
            }
            else if (resourceLink.Scheme.Equals("sftp", StringComparison.OrdinalIgnoreCase))
            {
                downloader = _sftp;
            }
            else
            {
                throw new Exception(Constants.INVALID_SCHEME_ERROR);
            }

            return downloader;
        }
    }
}