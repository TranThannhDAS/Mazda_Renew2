using Mazda.Data.Repository;

namespace Mazda.Data.UnitofWork
{
    public interface IUnitofWork
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

        Task<int> Complete();
    }
}
