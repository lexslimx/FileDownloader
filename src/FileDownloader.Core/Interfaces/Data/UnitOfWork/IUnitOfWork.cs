using FileDownloader.Core.Interfaces.Data.Repositories;
using FileDownloader.Core.SharedKernel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Core.Interfaces.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;

        Task<int> Commit(CancellationToken ct);
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : class
    {
        TContext Context { get; }
    }
}