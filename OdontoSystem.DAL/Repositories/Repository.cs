using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using OdontoSystem.DAL.Context;

namespace OdontoSystem.DAL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly OdontoContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(OdontoContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll() => _dbSet.ToList();
        public T GetById(object id) => _dbSet.Find(id);
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate).ToList();
        public void Add(T entity) => _dbSet.Add(entity);
        public void Update(T entity) => _context.Entry(entity).State = EntityState.Modified;
        public void Delete(T entity) => _dbSet.Remove(entity);
        public int SaveChanges() => _context.SaveChanges();
    }
}