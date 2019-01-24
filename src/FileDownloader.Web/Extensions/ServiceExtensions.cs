using FileDownloader.Core.Helpers;
using FileDownloader.Core.Interfaces.Data.Repositories;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.Interfaces.Services;
using FileDownloader.Core.Services;
using FileDownloader.Core.Settings;
using FileDownloader.Infrastructure.Data.Context;
using FileDownloader.Infrastructure.Data.Repositories;
using FileDownloader.Infrastructure.Data.UnitOfWork;
using FileDownloader.Web.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileDownloader.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureRouting(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        }

        public static void ConfigureDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        }

        public static void ConfigureMVC(this IServiceCollection services)
        {
            services.AddMvc();
        }

        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ExceptionFilterAttribute>();
            services.AddScoped<ValidateModelAttribute>();

            services.AddUnitOfWork<ApplicationDbContext>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IFileDownloadRepository, FileDownloadRepository>();

            services.AddScoped<IFileDownloadService, FileDownloadService>();

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

        public static void RegisterSettings(this IServiceCollection services, IConfiguration Configuration)
        {
            IConfigurationSection mainSection = Configuration.GetSection("App");
            services.Configure<ApplicationSettings>(mainSection);
        }
    }
}