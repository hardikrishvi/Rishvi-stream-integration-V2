using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Extensions;

namespace Rishvi.Modules.Core.Data
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);

        void Delete(object Id);

        void Delete(TEntity entity);

        IQueryable<TEntity> Get();

        IQueryable<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

        TEntity GetById(object id);

        void Update(TEntity entity);

        void UpdateList(IList<TEntity> entities);

        IQueryable<TEntity> AsNoTracking();

        Task AddAsync(TEntity entity);
        Task DeleteAsync(object id);
        Task DeleteAsync(TEntity entity);
        ValueTask<TEntity> GetByIdAsync(object id);
        Task UpdateAsync(TEntity entity);
        Task UpdateListAsync(IList<TEntity> entities);

    }

    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dbSet = _unitOfWork.Context.Set<TEntity>();
        }

        public void Add(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public void Delete(object id)
        {
            TEntity existing = _dbSet.Find(id);
            if (existing != null)
            {
                _dbSet.Remove(existing);
            }
        }

        public void Delete(TEntity entity)
        {
            TEntity existing = _dbSet.Find(entity);
            if (existing != null)
            {
                _dbSet.Remove(existing);
            }
        }

        public IQueryable<TEntity> Get()
        {
            return _dbSet.AsQueryable<TEntity>();
        }

        public TEntity GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public IQueryable<TEntity> Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate).AsQueryable<TEntity>();
        }

        public void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateList(IList<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                var entry = _unitOfWork.Context.Entry(entity);
                entry.State = EntityState.Modified;
            }
        }

        public IQueryable<TEntity> AsNoTracking()
        {
            return _dbSet.AsNoTracking();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task DeleteAsync(object id)
        {
            TEntity existing = await _dbSet.FindAsync(id);
            if (existing != null)
            {
                _dbSet.Remove(existing);
            }
        }

        public async Task DeleteAsync(TEntity entity)
        {
            TEntity existing = await _dbSet.FindAsync(entity);
            if (existing != null)
            {
                _dbSet.Remove(existing);
            }
        }

        public async ValueTask<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            await Task.Run(() => { Update(entity); });
        }

        public async Task UpdateListAsync(IList<TEntity> entities)
        {
            await Task.Run(() =>
            {
                UpdateList(entities);
            });

        }
    }
}