namespace OrdrMate.Repositories;

using OrdrMate.Models;

public interface IUserRepo
{

    Task<IEnumerable<User>> GetAll();
    Task<User?> GetUserByUsername(string username);
    Task<User?> GetUserById(string id);
    Task<User> CreateUser(User user);
    Task<bool> IsUsernameTaken(string username);
}