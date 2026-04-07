using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using CH.Data;

namespace CH.Business
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<List<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null);
        Task<TEntity> GetAsync(params object[] id);
        void Insert(TEntity entity);
        void Delete(TEntity entity);
        void Update(TEntity entity);
    }

    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            this._context = context;
            this._dbSet = this._context.Set<TEntity>();
        }

        public void Delete(TEntity entity)
        {
            this._dbSet.Remove(entity);
        }

        public async Task<List<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter != null)
            {
                query.Where(filter);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }

            return await query.ToListAsync();
        }

        public async Task<TEntity> GetAsync(params object[] id)
        {
            return await _dbSet.FindAsync(id);
        }

        public void Insert(TEntity entity)
        {
            this._dbSet.Add(entity);
        }

        public void Update(TEntity entity)
        {
            this._context.Entry(entity).State = EntityState.Modified;
        }
    }
}
