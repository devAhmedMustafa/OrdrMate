using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

namespace OrdrMate.Repositories;

public class UserRepo(OrdrMateDbContext c) : IUserRepo
{
    private readonly OrdrMateDbContext _db = c;

    public async Task<IEnumerable<User>> GetAll()
    {
        return await _db.User.ToListAsync();
    }

    public async Task<User?> GetUserById(string id)
    {
        return await _db.User.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<User> CreateUser(User user)
    {
        _db.User.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _db.User.FirstOrDefaultAsync(m => m.Username == username);
    }

    public async Task<bool> IsUsernameTaken(string username)
    {
        return await _db.User.AnyAsync(u => u.Username == username);
    }
    
    public async Task<User> UpdateUser(User user)
    {
        _db.User.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }
}