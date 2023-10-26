using Database.Interfaces;
using Database.Mics;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Database
{
    public class IntRepository<TEntity> : IIntRepository<TEntity> where TEntity : class, IIntEntity
    {
        private readonly ApiDbContext _dbContext;

        protected IntRepository(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter)
        {
            return await _dbContext.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(filter);
        }
        public IQueryable<TEntity> GetAll()
        {
            return _dbContext.Set<TEntity>().AsNoTracking();
        }
        public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> sortBy,
    int? skip, int? take)
        {
            return _dbContext.Set<TEntity>().AsNoTracking().Where(filter).OrderBy(sortBy).Skip(skip ?? DefaultValues.Skip).Take(take ?? DefaultValues.Take);
        }

        public async Task<TEntity> Create(TEntity entity)
        {
            var created = await _dbContext.Set<TEntity>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return created.Entity;
        }

        public async Task<TEntity> Update(int id, TEntity entity)
        {
            var updated = _dbContext.Set<TEntity>().Update(entity);
            await _dbContext.SaveChangesAsync();
            return updated.Entity;
        }

        public async Task LoadCollection(TEntity entity, Expression<Func<TEntity, IEnumerable<object>>> expression)
        {
            _dbContext.Entry(entity).State = EntityState.Unchanged;
            await _dbContext.Entry(entity).Collection(expression).LoadAsync();

        }

        public async Task LoadReference(TEntity entity, Expression<Func<TEntity, object>> expression)
        {
            _dbContext.Entry(entity).State = EntityState.Unchanged;
            await _dbContext.Entry(entity).Reference(expression).LoadAsync();
        }
    }
}
