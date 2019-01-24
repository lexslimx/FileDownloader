using FileDownloader.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileDownloader.Infrastructure.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<FileDownload> FileDownload { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FileDownload>(entity =>
            {
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(64);
            });
        }
    }
}