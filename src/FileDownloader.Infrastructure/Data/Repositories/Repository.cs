using FileDownloader.Core.Interfaces.Data.Repositories;
using FileDownloader.Core.SharedKernel;
using FileDownloader.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _db;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<T>> GetAll(CancellationToken ct)
        {
            return await _db.Set<T>().ToListAsync(ct);
        }

        public Task<T> Get(int id, CancellationToken ct)
        {
            return _db.Set<T>().SingleOrDefaultAsync(m => m.Id == id, ct);
        }

        public Task Add(T entity, CancellationToken ct)
        {
            return _db.Set<T>().AddAsync(entity, ct);
        }

        public Task AddRange(List<T> entities, CancellationToken ct)
        {
            return _db.Set<T>().AddRangeAsync(entities, cancellationToken: ct);
        }

        public void Update(T entity)
        {
            _db.Set<T>().Update(entity);
        }

        public async Task Remove(int id, CancellationToken ct)
        {
            T entity = await _db.Set<T>().SingleOrDefaultAsync(m => m.Id == id, ct);

            _db.Set<T>().Remove(entity);
        }
    }
}