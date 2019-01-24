using FileDownloader.Core.Helpers;
using FileDownloader.Core.Interfaces.Data.Repositories;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.Models;
using FileDownloader.Core.Services;
using FileDownloader.Core.Settings;
using FileDownloader.Infrastructure.Data.Context;
using FileDownloader.Infrastructure.Data.Repositories;
using FileDownloader.Infrastructure.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileDownloader.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceProvider services = Startup();

            IDownloadCreator downloader = services.GetService<IDownloadCreator>();

            List<FileDownloadRequest> fileDownloadRequests = new List<FileDownloadRequest>()
            {
                new FileDownloadRequest("ftp://nlls3.a2hosting.com/public_html/file.txt", new NetworkCredentials() { Username = "sportsho", Password = "7h3Q57Xaio" }),
                new FileDownloadRequest("http://weblos.net/html/softaco/index-v1.html"),
                new FileDownloadRequest("sftp://test.rebex.net//pub/example/ConsoleClient.png", new NetworkCredentials() { Username = "demo", Password = "password" })
            };

            foreach (FileDownloadRequest fileDownloadRequest in fileDownloadRequests)
            {
                Console.WriteLine($"Downloading {fileDownloadRequest.ResourceLink}...");

                DownloadResult downloadResult = downloader.DownloadAsync(fileDownloadRequest).Result;

                Console.WriteLine($"Success: {downloadResult.IsSuccess}");
                Console.WriteLine($"File path: {downloadResult.FilePath}");
                Console.WriteLine($"Size: {downloadResult.Size} bytes");
                Console.WriteLine($"Time elapsed: {downloadResult.TimeElapsed.Milliseconds} ms");
                Console.WriteLine($"Concurrent downloads: {downloadResult.ConcurrentDownloads}");

                if (!downloadResult.IsSuccess)
                {
                    Console.WriteLine($"Error: {downloadResult.Error}");
                }

                Console.WriteLine("");
            }

            Console.WriteLine("Press enter to exit..");

            Console.ReadLine();
        }

        public static ServiceProvider Startup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", false, true)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            IServiceCollection services = new ServiceCollection()
             .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
             .AddOptions()
             .Configure<ApplicationSettings>(configuration.GetSection("App"));

            services.AddServices();

            return services.BuildServiceProvider();
        }
    }

    public static class Extensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddUnitOfWork<ApplicationDbContext>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IFileDownloadRepository, FileDownloadRepository>();
            services.AddScoped<HttpFileDownloader>();
            services.AddScoped<FtpFileDownloader>();
            services.AddScoped<SftpFileDownloader>();
            services.AddScoped<IDownloadCreator, DownloadCreator>();
            services.AddSingleton<FileHelper>();
        }

        public static void AddUnitOfWork<TContext>(this IServiceCollection services) where TContext : ApplicationDbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
        }
    }
}