using FileDownloader.Core.Entities;
using FileDownloader.Core.Helpers;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.Models;
using FileDownloader.Core.Services;
using FileDownloader.Core.Settings;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FileDownloader.Tests
{
    public class DownloadCreatorTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private DownloadCreator _downloadCreator;
        private FileHelper _fileHelper;

        private OptionsWrapper<ApplicationSettings> options = new OptionsWrapper<ApplicationSettings>(new ApplicationSettings()
        {
            Url = "https://localhost:44327/DownloadedFiles",
            DestinationFolder = "C:\\temp"
        });

        public DownloadCreatorTests()
        {
            _fileHelper = new FileHelper();

            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _downloadCreator = new DownloadCreator(
               new FtpFileDownloader(_mockUnitOfWork.Object, options, _fileHelper),
               new HttpFileDownloader(_mockUnitOfWork.Object, options, _fileHelper),
               new SftpFileDownloader(_mockUnitOfWork.Object, options, _fileHelper)
               );
        }

        [Fact]
        public void TestHttpDownloaderIsPicked()
        {
            IDownloader result = _downloadCreator.Create(new Uri("http://weblos.net/html/softaco/index-v1.html"));
            Assert.IsType<HttpFileDownloader>(result);
        }

        [Fact]
        public void TestFtpDownloaderIsPicked()
        {
            IDownloader result = _downloadCreator.Create(new Uri("ftp://nlls3.a2hosting.com/public_html/file.txt"));
            Assert.IsType<FtpFileDownloader>(result);
        }

        [Fact]
        public void TestSftpDownloaderIsPicked()
        {
            IDownloader result = _downloadCreator.Create(new Uri("sftp://test.rebex.net//pub/example/ConsoleClient.png"));
            Assert.IsType<SftpFileDownloader>(result);
        }

        [Fact]
        public void TestUnhandledDownloaderIsPicked()
        {
            Assert.Throws<Exception>(() => _downloadCreator.Create(new Uri("some-unhandled-scheme://test.rebex.net//pub/example/ConsoleClient.png")));
        }

        [Fact]
        public void TestHttpFileDownload()
        {
            _mockUnitOfWork.Setup(x => x.GetRepository<FileDownload>().Add(It.IsAny<FileDownload>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            DownloadResult result = _downloadCreator.DownloadAsync(new FileDownloadRequest("http://weblos.net/html/softaco/index-v1.html")).Result;

            _mockUnitOfWork.Verify(_unitOfWork => _unitOfWork.GetRepository<FileDownload>().Add(It.IsAny<FileDownload>(), It.IsAny<CancellationToken>()), Times.Once());
            _mockUnitOfWork.Verify(_unitOfWork => _unitOfWork.Commit(It.IsAny<CancellationToken>()));

            Assert.True(result.IsSuccess);

            Assert.True(File.Exists(result.FilePath));

            FileInfo file = new FileInfo(result.FilePath);
            Assert.Equal(file.Length, result.Size);
        }

        [Fact]
        public void TestFtpFileDownload()
        {
            _mockUnitOfWork.Setup(x => x.GetRepository<FileDownload>().Add(It.IsAny<FileDownload>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            DownloadResult result = _downloadCreator.DownloadAsync(new FileDownloadRequest("ftp://nlls3.a2hosting.com/public_html/file.txt", new NetworkCredentials() { Username = "sportsho", Password = "7h3Q57Xaio" })).Result;

            _mockUnitOfWork.Verify(_unitOfWork => _unitOfWork.GetRepository<FileDownload>().Add(It.IsAny<FileDownload>(), It.IsAny<CancellationToken>()), Times.Once());
            _mockUnitOfWork.Verify(_unitOfWork => _unitOfWork.Commit(It.IsAny<CancellationToken>()));

            Assert.True(result.IsSuccess);

            Assert.True(File.Exists(result.FilePath));

            FileInfo file = new FileInfo(result.FilePath);
            Assert.Equal(file.Length, result.Size);
        }

        [Fact]
        public void TestSftpFileDownload()
        {
            _mockUnitOfWork.Setup(x => x.GetRepository<FileDownload>().Add(It.IsAny<FileDownload>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            DownloadResult result = _downloadCreator.DownloadAsync(new FileDownloadRequest("sftp://test.rebex.net//pub/example/ConsoleClient.png", new NetworkCredentials() { Username = "demo", Password = "password" })).Result;

            _mockUnitOfWork.Verify(_unitOfWork => _unitOfWork.GetRepository<FileDownload>().Add(It.IsAny<FileDownload>(), It.IsAny<CancellationToken>()), Times.Once());
            _mockUnitOfWork.Verify(_unitOfWork => _unitOfWork.Commit(It.IsAny<CancellationToken>()));

            Assert.True(result.IsSuccess);

            Assert.True(File.Exists(result.FilePath));

            FileInfo file = new FileInfo(result.FilePath);
            Assert.Equal(file.Length, result.Size);
        }
    }
}