using FileDownloader.Core.Entities;
using FileDownloader.Core.Helpers;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.Models;
using FileDownloader.Core.Settings;
using FileDownloader.Core.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Core.Services
{
    public class HttpFileDownloader : IDownloader
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationSettings _applicationSettings;
        private readonly FileHelper _fileHelper;

        public HttpFileDownloader(IUnitOfWork uow,
            IOptions<ApplicationSettings> applicationSettings,
            FileHelper fileHelper)
        {
            _uow = uow;
            _applicationSettings = applicationSettings.Value;
            _fileHelper = fileHelper;
        }

        public async Task<DownloadResult> DownloadAsync(FileDownloadRequest fileDownloadRequest)
        {
            int nbOfConcurrentDownloads = 4;
            string destinationFolderPath = _applicationSettings.DestinationFolder;

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

                FileInfo file = _fileHelper.SetupFile(uri, destinationFolderPath);
                destinationFilePath = file.FullName;

                responseLength = await GetContentLengthAsync(uri);

                List<Range> readRanges = FillRanges(nbOfConcurrentDownloads, responseLength);

                Task[] tasks = new Task[nbOfConcurrentDownloads];
                object obj = new object();

                using (FileStream fileStream = file.Create())
                {
                    ConcurrentDownload(nbOfConcurrentDownloads, tasks, obj, readRanges, fileDownloadRequest.ResourceLink, fileStream);

                    await Task.WhenAll(tasks);
                }

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

        private async Task<long> GetContentLengthAsync(Uri uri)
        {
            long responseLength;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
                responseLength = response.Content.Headers.ContentLength ?? 0;
            }

            return responseLength;
        }

        private List<Range> FillRanges(int nbOfConcurrentDownloads, long responseLength)
        {
            List<Range> readRanges = new List<Range>();

            for (int chunk = 0; chunk < nbOfConcurrentDownloads - 1; chunk++)
            {
                Range range = new Range()
                {
                    Start = chunk * (responseLength / nbOfConcurrentDownloads),
                    End = ((chunk + 1) * (responseLength / nbOfConcurrentDownloads)) - 1
                };
                readRanges.Add(range);
            }

            readRanges.Add(new Range()
            {
                Start = readRanges.Any() ? readRanges.Last().End + 1 : 0,
                End = responseLength - 1
            });

            return readRanges;
        }

        private void ConcurrentDownload(int nbOfConcurrentDownloads, Task[] tasks, object obj, List<Range> readRanges, Uri uri, FileStream filestream)
        {
            for (int i = 0; i < nbOfConcurrentDownloads; i++)
            {
                int i1 = i;
                tasks[i] = Task.Run(async () =>
                {
                    using (HttpClient httpclient = new HttpClient())
                    using (HttpRequestMessage httprequest = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        httprequest.Headers.Add("Range", $"bytes={readRanges[i1].Start}-{readRanges[i1].End}");
                        HttpResponseMessage response = await httpclient.SendAsync(httprequest);
                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        {
                            lock (obj)
                            {
                                filestream.Position = readRanges[i1].Start;
                                stream.CopyTo(filestream);
                            }
                        }
                    }
                });
            }
        }
    }
}