using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Database.Interfaces
{
    public interface IIntRepository<TEntity> where TEntity : class, IIntEntity
    {
        Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter);
        IQueryable<TEntity> GetAll();

        IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> sortBy,
    int? skip, int? take);

        Task<TEntity> Create(TEntity entity);
        Task<TEntity> Update(int id, TEntity entity);

        Task LoadCollection(TEntity entity, Expression<Func<TEntity, IEnumerable<object>>> expression);
        Task LoadReference(TEntity entity, Expression<Func<TEntity, object>> expression);
    }
}
