using Mazda_Api.Controllers;
using Mazda_Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Mazda.Data.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private DataContext context;
        public GenericRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(T entity)
        {
             context.Set<T>().AddAsync(entity);
        }

        public async Task Delete(T entity)
        {
             context.Set<T>().Remove(entity);
        }

        public async Task DeleteRange(IReadOnlyList<T> entity)
        {
            context.Set<T>().RemoveRange(entity);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await context.Set<T>().FindAsync(id);

        }

        public async Task Update(T entity)
        {
            context.Set<T>().Update(entity);
        }
    }
}
