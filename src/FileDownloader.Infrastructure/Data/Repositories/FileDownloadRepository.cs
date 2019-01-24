using FileDownloader.Core.Entities;
using FileDownloader.Core.Interfaces.Data.Repositories;
using FileDownloader.Infrastructure.Data.Context;

namespace FileDownloader.Infrastructure.Data.Repositories
{
    public class FileDownloadRepository : Repository<FileDownload>, IFileDownloadRepository
    {
        public FileDownloadRepository(ApplicationDbContext _db)
            : base(_db)
        {
        }
    }
}