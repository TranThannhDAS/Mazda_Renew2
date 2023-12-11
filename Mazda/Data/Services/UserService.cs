using Mazda.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mazda.Data.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _db;

        public UserService(DataContext _db)
        {
            this._db = _db;
        }
        public async Task<User> IsValidUserAsync(string username, string password)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == username);
            if (user is not null)
            {
                if (user.Pass.Length > 0)
                {
                    if (BCrypt.Net.BCrypt.Verify(password, user.Pass))
                    {
                        return user;
                    }
                }
            }
            return null;
        }
    }
}
