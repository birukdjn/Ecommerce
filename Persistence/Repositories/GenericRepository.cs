using Domain.Common;
using Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context = context;

        public async Task<T?> GetByIdAsync(Guid id)
        {
            
            return await _context.Set<T>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<T?> GetByIdAsync() =>
            await _context.Set<T>().FindAsync();

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _context.Set<T>()
                .Where(x => !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<T?> GetWithDeletedAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(predicate);
        }

        public void Add(T entity) =>
            _context.Set<T>().Add(entity);

        public void Update(T entity) =>
            _context.Set<T>().Update(entity);

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.LastModifiedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
        }
    }
}
