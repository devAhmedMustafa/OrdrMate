using Xunit;
using OrdrMate.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using OrdrMate.Models;
using OrdrMate.Repositories;

public class UserRepoTests
{

    private readonly IUserRepo _userRepo;
    private readonly OrdrMateDbContext _context;

    public UserRepoTests()
    {

        var options = new DbContextOptionsBuilder<OrdrMateDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new OrdrMateDbContext(options);
        _userRepo = new UserRepo(_context);
    }

    [Fact]
    public async Task TestCreateUser()
    {
        var user = new User { Id = "1", Username = "Test User", Password = "password" };
        var createdUser = await _userRepo.CreateUser(user);
        Assert.NotNull(createdUser);
    }

    [Fact]
    public async Task TestGetUserById()
    {
        var retrievedUser = await _userRepo.GetUserById("1");
        Assert.NotNull(retrievedUser);
        Assert.Equal("Test User", retrievedUser.Username);
        Assert.NotEqual("password121", retrievedUser.Password);
    }

}