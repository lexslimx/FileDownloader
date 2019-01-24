using FileDownloader.Core.SharedKernel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader.Core.Interfaces.Data.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<List<T>> GetAll(CancellationToken ct);

        Task<T> Get(int Id, CancellationToken ct);

        Task Add(T entity, CancellationToken ct);

        Task AddRange(List<T> entities, CancellationToken ct);

        void Update(T entity);

        Task Remove(int Id, CancellationToken ct);
    }
}