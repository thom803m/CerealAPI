using Microsoft.EntityFrameworkCore;
using CerealAPI.Models;
using CerealAPI.Interfaces;
using CerealAPI.Data;

namespace CerealAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CerealContext _context;

        public UserRepository(CerealContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                                 .AsNoTracking()
                                 .SingleOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
