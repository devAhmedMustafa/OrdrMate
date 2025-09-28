using OrdrMate.Repositories;

namespace OrdrMate.Features.UserPassword;

public class UserPasswordService
{
    private readonly IUserRepo _userRepo;
    public UserPasswordService(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task ChangeUserPasswordAsync(ChangePasswordDto data)
    {
        var user = await _userRepo.GetUserById(data.UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(data.OldPassword, user.Password))
        {
            throw new Exception("Old password is incorrect");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(data.NewPassword);
        await _userRepo.UpdateUser(user);
    }
}