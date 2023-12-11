using Mazda_Api.Dtos;

namespace Mazda.Data.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);

        //Có điền kiện

        Task AddAsync(T entity);
        Task Update(T entity);
        Task Delete(T entity);
        Task DeleteRange(IReadOnlyList<T> entity);
    }
}
