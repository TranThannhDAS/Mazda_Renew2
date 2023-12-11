using Mazda.Model;

namespace Mazda.Data.Services
{
    public interface IUserService
    {
        Task<User> IsValidUserAsync(string username, string password);


    }
}
