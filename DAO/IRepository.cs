using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AccuBooks_ERP.GetterNSetter;

namespace AccuBooks_ERP.DAO
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        Task SoftDeleteAsync(int id, string? deletedBy = null);
        IQueryable<T> Query();
    }
}
