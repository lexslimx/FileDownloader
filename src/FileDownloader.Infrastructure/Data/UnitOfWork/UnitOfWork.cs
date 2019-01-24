using FileDownloader.Core.Interfaces.Data.Repositories;
using FileDownloader.Core.Interfaces.Data.UnitOfWork;
using FileDownloader.Core.SharedKernel;
using FileDownloader.Infrastructure.Data.Context;
using FileDownloader.Infrastructure.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Infrastructure.Data.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWork
        where TContext : ApplicationDbContext, IDisposable
    {
        public TContext Context { get; }
        private Dictionary<Type, object> _repositories;

        public UnitOfWork(TContext context)
        {
            Context = context;
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
        {
            if (_repositories == null)
                _repositories = new Dictionary<Type, object>();

            Type type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
                _repositories[type] = new Repository<TEntity>(Context);

            return (IRepository<TEntity>)_repositories[type];
        }

        public async Task<int> Commit(CancellationToken ct)
        {
            return await Context.SaveChangesAsync(ct);
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}